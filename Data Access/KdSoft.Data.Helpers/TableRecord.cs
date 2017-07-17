using Dapper;
using KdSoft.Reflection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KdSoft.Data.Helpers
{
    /// <summary>
    /// Case-insensitive dictionary for generating insert and update SQL for a given table from
    /// property names or <see cref="KeyValuePair{T,V}">KeyValuePair&lt;string,object></see> collections.
    /// Does not allow adding or removing properties once created.
    /// </summary>
    /// <remarks>Not thread safe.</remarks>
    public class TableRecord: IDictionary<string, object>
    {
        const string insertTemplate = @"INSERT INTO {0} ({1}) VALUES ({2}) SELECT CAST(SCOPE_IDENTITY() AS INT)";

        readonly string tableName;
        readonly IEnumerable<string> keys;
        readonly IDictionary<string, object> properties;
        string insertSql;
        string updateSql;

        public TableRecord(string tableName, IEnumerable<string> keys, dynamic template) {
            this.tableName = tableName;
            this.keys = keys.Select(k => k.ToUpperInvariant());
            this.properties = new Dictionary<string, object>();

            var obj = (object)template;

            var propValues = obj as IEnumerable<KeyValuePair<string, object>>;
            if (propValues == null)
                propValues = obj.GetPropertyValues();
            foreach (var propValue in propValues)
                properties.Add(propValue.Key.ToUpperInvariant(), propValue.Value);
        }

        public void AssignValues(dynamic values) {
            var obj = (object)values;

            var propValues = obj as IEnumerable<KeyValuePair<string, object>>;
            if (propValues == null)
                propValues = obj.GetPropertyValues();
            foreach (var propValue in propValues)
                this[propValue.Key.ToUpperInvariant()] = propValue.Value;
        }

        public string InsertSql {
            get {
                if (insertSql == null)
                    insertSql = GetInsertSql(tableName, properties.Keys);
                return insertSql;
            }
        }

        public string UpdateSql {
            get {
                if (updateSql == null)
                    updateSql = GetUpdateSql(tableName, keys, properties.Keys);
                return updateSql;
            }
        }

        public static string GetInsertSql(string tableName, ICollection<string> recordFields) {
            string columns = string.Join(",", recordFields);
            string valueParams = string.Join(",", recordFields.Select(k => "@" + k));
            string sql = string.Format(insertTemplate, tableName, columns, valueParams);
            return sql;
        }

        public static string GetUpdateSql(string tableName, IEnumerable<string> keys, ICollection<string> recordFields) {
            var assignmentFields = recordFields.Except(keys);

            var sb = new StringBuilder("UPDATE ");
            sb.Append(tableName);
            sb.Append(" SET ");
            foreach (var af in assignmentFields) {
                sb.AppendFormat("{0} = @{0}", af);
            }
            sb.Append(" WHERE ");

            bool next = false;
            foreach (var kf in keys) {
                if (next) sb.Append(" AND "); else next = true;
                sb.AppendFormat("{0} = @{0}", kf);
            }

            return sb.ToString();
        }

        public Task<IEnumerable<int>> InsertAsync(DbConnection conn) {
            return SqlMapper.QueryAsync<int>(conn, InsertSql, properties);
        }

        public IEnumerable<int> Insert(DbConnection conn) {
            return SqlMapper.Query<int>(conn, InsertSql, properties);
        }

        public Task<int> UpdateAsync(DbConnection conn) {
            return SqlMapper.ExecuteAsync(conn, UpdateSql, properties);
        }

        public int Update(DbConnection conn) {
            return SqlMapper.Execute(conn, UpdateSql, properties);
        }

        #region IDictionary<string, object>

        public void Add(string key, object value) {
            throw new InvalidOperationException();
        }

        public bool ContainsKey(string key) {
            return properties.ContainsKey(key);
        }

        public ICollection<string> Keys {
            get { return properties.Keys; }
        }

        public bool Remove(string key) {
            throw new InvalidOperationException();
        }

        public bool TryGetValue(string key, out object value) {
            string name = key.ToUpperInvariant();
            return properties.TryGetValue(name, out value);
        }

        public ICollection<object> Values {
            get { return properties.Values; }
        }

        public object this[string key] {
            get {
                string name = key.ToUpperInvariant();
                return properties[name];
            }
            set {
                string name = key.ToUpperInvariant();
                if (properties.ContainsKey(name))
                    properties[name] = value;
                else
                    throw new InvalidOperationException("Must not add new properties to TableRecord.");
            }
        }

        public void Add(KeyValuePair<string, object> item) {
            throw new InvalidOperationException();
        }

        public void Clear() {
            throw new InvalidOperationException();
        }

        public bool Contains(KeyValuePair<string, object> item) {
            return properties.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
            throw new InvalidOperationException();
        }

        public int Count {
            get { return properties.Count; }
        }

        public bool IsReadOnly {
            get { return properties.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, object> item) {
            throw new InvalidOperationException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return properties.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return properties.GetEnumerator();
        }

        #endregion
    }
}
