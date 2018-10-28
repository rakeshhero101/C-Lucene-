using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace RetrievalSystem
{
    class Searcher
    {
        Lucene.Net.Store.Directory indexDirectory;
        Lucene.Net.Analysis.Analyzer analyzer;
        Lucene.Net.Index.IndexWriter writer;
        Lucene.Net.Search.IndexSearcher searcher;
        Lucene.Net.QueryParsers.QueryParser parser;
        public Query query { get; set; }

        const Lucene.Net.Util.Version VERSION = Lucene.Net.Util.Version.LUCENE_30;
        const string TEXT_FN = "World";

        public Searcher(string indexPath, Lucene.Net.Analysis.Analyzer analyzer, Lucene.Net.Index.IndexWriter writer)
        {
            this.indexDirectory = Lucene.Net.Store.FSDirectory.Open(indexPath);
            this.analyzer = analyzer;
            this.writer = writer;
        }

        public void CreateSearcher()
        {
            searcher = new Lucene.Net.Search.IndexSearcher(this.indexDirectory);
        }

        public void CreateParser(String type, String field)
        {
            //parser = new Lucene.Net.QueryParsers.QueryParser(VERSION, TEXT_FN, analyzer);
            

            switch (type)
            {
                case "Single Term":
                    parser = new Lucene.Net.QueryParsers.QueryParser(VERSION, field, analyzer);
                    break;
                case "Muti-Term":
                    string[] fields = new[] { "DocID", "Title", "Author", "Bibliographic", "Words" };
                    parser = new Lucene.Net.QueryParsers.MultiFieldQueryParser(VERSION, fields, analyzer);
                    break;
                /*case "Muti - Phrase":
                    parser = new Lucene.Net.QueryParsers.mu(VERSION, fields, analyzer);
                    break;*/
            }
            
            //parser = new Lucene.Net.QueryParsers.QueryParser(VERSION, TEXT_FN, analyzer);
        }

        public Lucene.Net.Search.TopDocs SearchIndex(string text)
        {
            text.ToLower();
            query = parser.Parse(text);
            Lucene.Net.Search.TopDocs doc = searcher.Search(query, 1000);
            searcher.Dispose();
            return doc;
        }

        public Dictionary<string, float> DisplayResults(Lucene.Net.Search.TopDocs docs, List<Collection> collections)
        {
            Dictionary<string, float> resultList = new Dictionary<string, float>();
            // Get the doc ids for collections
            string[] collection_DocIds = collections.Select(n => n.DocID).ToArray();
            // Get the doc ids for searching result
            foreach (Lucene.Net.Search.ScoreDoc doc in docs.ScoreDocs)
            {
               
               resultList.Add(collection_DocIds[doc.Doc], doc.Score);
               
            }
            return resultList;
        }
    }
}
