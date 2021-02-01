using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nerine.Collections.Tables;
using Nerine.Exceptions;

namespace Nerine.Collections
{
    public class QueryBuilder
    {
        private Collection collection;
        private string[] currentValues;
        private Dictionary<string, object> currentIValues;
        private string currentTable;
        private QueryType type;
        private bool queryStarted = false;

        private static List<QueryType> FROM_QUERIES = new List<QueryType>()
        {
            QueryType.Select
        };

        private static List<QueryType> INTO_QUERIES = new List<QueryType>()
        {
            QueryType.Insert
        };

        // predicates
        private Func<Dictionary<string, object>, bool> wherePredicate;

        public QueryBuilder(Collection coll)
        {
            collection = coll;
        }

        public QueryBuilder Select(string[] values)
        {
            queryStarted = true;
            type = QueryType.Select;
            currentValues = values;

            return this;
        }

        public QueryBuilder Insert(Dictionary<string, object> values)
        {
            queryStarted = true;
            type = QueryType.Insert;
            currentValues = values;

            return this;
        }

        public QueryBuilder From(string table)
        {
            if (!queryStarted || FROM_QUERIES.Find(x => x == type) == null)
                return null;

            currentTable = table;

            return this;
        }

        public QueryBuilder Into(string table)
        {
            if (!queryStarted || INTO_QUERIES.Find(x => x == type) == null)
                return null;

            currentTable = table;

            return this;
        }

        public QueryBuilder Where(Func<Dictionary<string, object>, bool> where)
        {
            if (!queryStarted)
                return null;

            wherePredicate = where;

            return this;
        }

        public Query End()
        {
            // end query
            queryStarted = false;
            var table = collection.GetTable(currentTable);
            List<int> indices = new List<int>();
            List<Row> rows = new List<Row>();

            // check
            if (type == QueryType.Select)
            {
                if (wherePredicate != null)
                {
                    foreach (var row in table.Rows)
                    {
                        Dictionary<string, object> values = new Dictionary<string, object>();

                        foreach (var index in currentValues)
                        {
                            values.Add(index, row.Values[index]);
                        }

                        if (wherePredicate(values) == true)
                        {
                            rows.Add(new Row()
                            {
                                Values = values
                            });
                        }
                    }
                }
                else
                {
                    foreach (var row in table.Rows)
                    {
                        Dictionary<string, object> values = new Dictionary<string, object>();

                        foreach (var index in currentValues)
                        {
                            values.Add(index, row.Values[index]);
                        }

                        rows.Add(new Row()
                        {
                            Values = values
                        });
                    }
                }

                return new Query()
                {
                    Results = rows,
                    Type = type
                };
            } else if (type == QueryType.Insert)
            {
                rows.Add(table.Insert(currentIValues));
                return new Query()
                {
                    Results = rows,
                    Type = type
                };
            }
            else
            {
                return null;
            }
        }
    }

    public enum QueryType
    {
        Select = 1,
        Insert = 2
    }

    public class Query
    {
        public List<Row> Results;

        public QueryType Type;
    }
}
