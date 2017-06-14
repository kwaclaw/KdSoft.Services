using KdSoft.Services.StorageServices;
using KdSoft.Services.StorageServices.Transient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using claims = System.Security.Claims;

namespace KdSoft.Services.Security
{
    public struct PropertyValue
    {
        public PropertyValue(int index, byte[] value): this() {
            this.Index = index;
            this.Value = value;
        }

        public int Index { get; private set; }
        public byte[] Value { get; private set; }
    }

    public delegate string ClaimTypeDecoder(byte[] bytes);
    public delegate string[] ClaimTypeMultiDecoder(byte[] bytes);

    public class ClaimDesc
    {
        public readonly PropDesc PropDesc;
        public readonly string ValueType;  // claim value type Uri
        public readonly ClaimTypeDecoder Decoder;
        public readonly ClaimTypeMultiDecoder MultiDecoder;

        public ClaimDesc(PropDesc propDesc, string valueType, ClaimTypeDecoder decoder, ClaimTypeMultiDecoder multiDecoder = null) {
            if ((decoder == null && multiDecoder == null) || (decoder != null && multiDecoder != null))
                throw new ArgumentException("Exactly one of decoder and multiDecoder must be null.");
            this.PropDesc = propDesc;
            this.ValueType = valueType;
            this.Decoder = decoder;
            this.MultiDecoder = multiDecoder;
        }
    }

    public interface IClaimsCacheConfig
    {
        TransientStorageManager StorageMgr { get; }
        TimeSpan ClaimsTimeout { get; }
        TimeSpan LockTimeout { get; }
        TimeSpan MaxLockWaitTime { get; }
    }

    public abstract class ClaimsCache
    {
        protected TransientStore Store { get; private set; }
        protected ArraySegment<PropRequest> AllCreateLockRequests { get; private set; }
        protected ArraySegment<PropRequest> AllReadLockRequests { get; private set; }
        protected int MaxLockWaitTimeSeconds { get; private set; }
        protected readonly byte[] emptyBuffer = new byte[0];

        protected struct ClaimTypeDesc
        {
            public readonly string ValueType;  // claim value type Uri
            public readonly ClaimTypeDecoder Decoder;
            public readonly ClaimTypeMultiDecoder MultiDecoder;
            public readonly int PropertyIndex;

            public ClaimTypeDesc(ClaimDesc claimDesc, int propertyIndex) {
                this.ValueType = claimDesc.ValueType;
                this.Decoder = claimDesc.Decoder;
                this.MultiDecoder = claimDesc.MultiDecoder;
                this.PropertyIndex = propertyIndex;
            }
        }

        protected Dictionary<string, ClaimTypeDesc> ClaimTypeDescs { get; private set; }

        public ClaimsCache(string name, IClaimsCacheConfig config) {
            this.MaxLockWaitTimeSeconds = (int)config.MaxLockWaitTime.TotalSeconds;

            ClaimTypeDescs = new Dictionary<string, ClaimTypeDesc>();

            var claimDescs = GetClaimDescriptions();
            var propDescs = GetPropertyDescriptions();

            var propertyDescs = new PropDesc[claimDescs.Count + propDescs.Count];
            int propertyIndex = 0;
            for (int indx = 0; indx < claimDescs.Count; indx++) {
                var cpDesc = claimDescs[indx];
                propertyDescs[propertyIndex] = cpDesc.PropDesc;
                ClaimTypeDescs[cpDesc.PropDesc.Name] = new ClaimTypeDesc(cpDesc, propertyIndex);
                propertyIndex++;
            }

            PropertiesStartIndex = propertyIndex;
            for (int indx = 0; indx < propDescs.Count; indx++) {
                propertyDescs[propertyIndex++] = propDescs[indx];
            }

            Store = new TransientStore(
                config.StorageMgr,
                name,
                propertyDescs,
                (int)config.ClaimsTimeout.TotalSeconds,
                (int)config.LockTimeout.TotalSeconds
            );

            var createLockRequests = new PropRequest[propertyDescs.Length];
            for (int i = 0; i < createLockRequests.Length; i++)
                createLockRequests[i] = new PropRequest(i, LockMode.Create);
            AllCreateLockRequests = new ArraySegment<PropRequest>(createLockRequests);

            var readLockRequests = new PropRequest[propertyDescs.Length];
            for (int i = 0; i < createLockRequests.Length; i++)
                readLockRequests[i] = new PropRequest(i, LockMode.Read);
            AllReadLockRequests = new ArraySegment<PropRequest>(readLockRequests);
        }

        protected abstract IList<ClaimDesc> GetClaimDescriptions();

        protected abstract IList<PropDesc> GetPropertyDescriptions();

        public int PropertiesStartIndex { get; private set; }

        protected ArraySegment<PropEntry> ClonePropEntries(ArraySegment<PropEntry> entries) {
            var newEntries = new PropEntry[entries.Count];
            Array.Copy(entries.Array, entries.Offset, newEntries, 0, newEntries.Length);
            return new ArraySegment<PropEntry>(newEntries);
        }

        static void SetPropEntryValue(ref PropEntry entry, PropertyValue propValue) {
            Debug.Assert(entry.Index == propValue.Index);
            entry.Value = propValue.Value;
        }

        protected PropertyValue CreatePropValue(string claimType, byte[] value) {
            int propIndex = ClaimTypeDescs[claimType].PropertyIndex;
            return new PropertyValue(propIndex, value);
        }

        protected PropertyValue CreateStringPropValue(string claimType, string value) {
            int propIndex = ClaimTypeDescs[claimType].PropertyIndex;
            var bytes = value == null ? emptyBuffer : Encoding.UTF8.GetBytes(value);
            return new PropertyValue(propIndex, bytes);
        }

        public async Task<ArraySegment<PropEntry>> StorePropertyValuesAsync(byte[] claimsId, IList<PropertyValue> propValues) {
            var createLockRequests = new PropRequest[propValues.Count];
            for (int i = 0; i < createLockRequests.Length; i++)
                createLockRequests[i] = new PropRequest(propValues[i].Index, LockMode.Create);
            var getResult = await Store.GetAsync(claimsId, new ArraySegment<PropRequest>(createLockRequests), MaxLockWaitTimeSeconds, false);
            if (getResult.Status != ErrorCode.None)
                throw new ClaimsCacheException(string.Format("Claims Cache Error: {0}.", getResult.Status.ToString()), (int)getResult.Status);

            Exception populateEx = null;
            var entries = getResult.Values;
            try {
                int limit = entries.Offset + entries.Count;
                for (int i = entries.Offset; i < limit; i++) {
                    // must pass struct by reference to avoid assigning to a copy of the struct
                    SetPropEntryValue(ref entries.Array[i], propValues[i]);
                }
            }
            catch (Exception ex) {
                populateEx = ex;
            }

            // this clears the locks, but must run outside of a finally block
            var errorCode = await Store.PutAsync(claimsId, entries);
            if (populateEx != null)  // throw previous exception, if there was one
                throw populateEx;
            if (errorCode != ErrorCode.None)
                throw new ClaimsCacheException(string.Format("Claims Cache Error: {0}.", errorCode.ToString()), (int)errorCode);

            return entries;
        }

        //public  IList<PropertyValue> CreatePropertyValues(IList<claims.Claim> claims) {
        //    var result = new List<PropertyValue>(claims.Count);
        //    for (int indx = 0; indx < claims.Count; indx++) {
        //        var claim = claims[indx];
        //        var claimTypeDesc = ClaimTypeDescs[claim.Type];
        //        var bytes = claimTypeDesc.Encoder(claim.Value);
        //        result.Add(new PropertyValue(claimTypeDesc.PropertyIndex, bytes));
        //    }
        //    return result;
        //}

        public IList<claims.Claim> CreateClaims(ArraySegment<PropEntry> entries) {
            var result = new List<claims.Claim>(entries.Count);
            int limit = entries.Offset + entries.Count;
            for (int indx = entries.Offset; indx < limit; indx++) {
                byte[] bytes = entries.Array[indx].Value;
                if (bytes == null)
                    continue;

                var propIndex = entries.Array[indx].Index;
                if (propIndex >= PropertiesStartIndex)
                    throw new ClaimsCacheException(string.Format("Property index not valid for a claim: {0}.", propIndex));

                var propDesc = Store.PropDescs[propIndex];
                var claimTypeDesc = ClaimTypeDescs[propDesc.Name];

                if (claimTypeDesc.Decoder != null) {
                    var stringValue = claimTypeDesc.Decoder(bytes);
                    var claim = new claims.Claim(propDesc.Name, stringValue, claimTypeDesc.ValueType);
                    result.Add(claim);
                }
                else {
                    var stringValues = claimTypeDesc.MultiDecoder(bytes);
                    for (int ci = 0; ci < stringValues.Length; ci++) {
                        var claim = new claims.Claim(propDesc.Name, stringValues[ci], claimTypeDesc.ValueType);
                        result.Add(claim);
                    }
                }
            }
            return result;
        }

        public async Task<ArraySegment<PropEntry>> GetPropertyValuesAsync(byte[] claimsId, IList<int> propIndexes = null) {
            ArraySegment<PropRequest> readLockRequests;
            if (propIndexes == null) {
                readLockRequests = AllReadLockRequests;
            }
            else {
                var rlRequests = new PropRequest[propIndexes.Count];
                for (int i = 0; i < rlRequests.Length; i++)
                    rlRequests[i] = new PropRequest(propIndexes[i], LockMode.Create);
                readLockRequests = new ArraySegment<PropRequest>(rlRequests);
            }

            var getResult = await Store.GetAsync(claimsId, readLockRequests, MaxLockWaitTimeSeconds, false);
            if (getResult.Status != ErrorCode.None)
                throw new ClaimsCacheException(string.Format("Claims Cache Error: {0}.", getResult.Status.ToString()), (int)getResult.Status);

            // we clone the PropEntry instances because further down we clear the values to reset the locks
            var clearLockEntries = ClonePropEntries(getResult.Values);

            // this clears the locks without updating the values, must be run outside a finally block
            int indxLimit = clearLockEntries.Offset + clearLockEntries.Count;
            for (int indx = clearLockEntries.Offset; indx < indxLimit; indx++) {
                // PropEnty is a struct, we better assign to its Value property directly
                clearLockEntries.Array[indx].Value = null;
            }
            var errorCode = await Store.PutAsync(claimsId, clearLockEntries);
            // we ignore lock id mismatches on overlapping read locks
            if (errorCode != ErrorCode.None && errorCode != ErrorCode.LockIdMismatch)
                throw new ClaimsCacheException(string.Format("Claims Cache Error: {0}.", errorCode.ToString()), (int)errorCode);

            return getResult.Values;
        }

        public async Task<IList<claims.Claim>> GetClaimsAsync(byte[] claimsId, IList<int> propIndexes = null) {
            var entries = await GetPropertyValuesAsync(claimsId, propIndexes);
            return CreateClaims(entries);
        }

        public async Task<bool> RemoveClaimsAsync(byte[] claimsId) {
            var deleteResult = await Store.DeleteAsync(claimsId, MaxLockWaitTimeSeconds, true);
            if (deleteResult.Status != ErrorCode.None)
                throw new ClaimsCacheException(string.Format("Claims Cache Error: {0}.", deleteResult.Status.ToString()), (int)deleteResult.Status);
            return deleteResult.Deleted;
        }

        public bool ClaimsExist(byte[] claimsId) {
            return Store.Exists(claimsId).Item1;
        }
    }
}
