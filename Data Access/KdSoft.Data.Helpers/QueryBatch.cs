using SqlModeller.Compiler.Model;
using SqlModeller.Model;
using SqlModeller.Shorthand;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using static Dapper.SqlMapper;

namespace KdSoft.Data.Helpers
{
    public class QueryBatch
    {
        public class Entry: BatchEntry<Query>
        {
            public Entry(Query query, string name = null) : base(query, name) { }
        }

        public string Header { get; set; }
        List<Entry> entries;

        ReadOnlyCollection<Entry> readonlyEntries;
        public ReadOnlyCollection<Entry> Entries {
            get {
                if (readonlyEntries == null)
                    readonlyEntries = new ReadOnlyCollection<Entry>(entries);
                return readonlyEntries;
            }
        }

        /// <summary>
        /// Adds query to batch.
        /// </summary>
        /// <param name="query">Query to add.</param>
        /// <returns>Index of query in batch.</returns>
        /// <remarks>Use the returned index to determine the order of results when running the batch statement.</remarks>
        public int AddEntry(Entry entry) {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            int result = entries.Count;
            entries.Add(entry);
            readonlyEntries = null;  // force re-creation
            return result;
        }

        public QueryBatch(IList<Entry> entries, string header = null) {
            if (entries == null)
                throw new ArgumentNullException(nameof(entries));
            this.entries = new List<Entry>(entries);
            this.Header = header;
        }


        public CompiledQueryBatch Compile(bool useParameters = false, string header = null) {
            var compiledEntries = new List<CompiledQueryBatch.Entry>();
            foreach (var entry in entries) {
                var compiledEntry = new CompiledQueryBatch.Entry(entry.Query.Compile(useParameters), entry.Name);
                compiledEntries.Add(compiledEntry);
            }
            if (header == null)
                header = Header;
            return new CompiledQueryBatch(compiledEntries, header);
        }
    }

    public class BatchEntry<T> where T : class
    {
        public BatchEntry(T query, string name = null) {
            this.Query = query;
            this.Name = name;
        }

        public string Name { get; private set; }
        public T Query { get; private set; }
    }


    public class CompiledQueryBatch
    {
        public class Entry: BatchEntry<CompiledQuery>
        {
            public Entry(CompiledQuery query, string name = null) : base(query, name) { }
        }

        public string Header { get; private set; }

        IList<Entry> entries;
        public IList<Entry> Entries { get { return entries; } }

        internal CompiledQueryBatch(IList<Entry> entries, string header) {
            this.entries = entries;
            this.Header = header;
        }

        string sql;
        public string Sql {
            get {
                if (sql != null)
                    return sql;

                var sb = new StringBuilder();
                sb.Append(Header);
                sb.AppendLine(";");

                foreach (var entry in entries) {
                    sb.Append(entry.Query.Sql);
                    sb.AppendLine(";");
                }
                sql = sb.ToString();
                return sql;
            }
        }
    }
}
