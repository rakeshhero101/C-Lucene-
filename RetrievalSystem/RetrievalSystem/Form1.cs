using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RetrievalSystem
{
    public partial class Form1 : Form
    {
        IndexGenerator generator = new IndexGenerator();
        List<string> col = new List<string>();
        public Form1()
        {
            InitializeComponent();
            ddl_Type.SelectedIndex = 0;
            ddl_Fields.SelectedIndex = 0;
            ReadInformationNeed();
        }

        private void ReadInformationNeed()
        {
            string informationNeedPath = @"./data/cran_information_needs.txt";

            // Read the files from the collection directory
            FileInfo di = new FileInfo(informationNeedPath);

            string text = System.IO.File.ReadAllText(di.FullName);

            // Get Words
            string[] stringSeparators = new string[] { ".I", "\n.I" };
            string[] result = text.Split(stringSeparators, StringSplitOptions.None);
            if (result.Count() >= 2)
            {
                result = result.Skip(1).ToArray();
            }

            
            foreach (string s in result)
            {
                string Temptext = s.Split(new string[] { ".D\n", "\n.D\n" }, StringSplitOptions.None)[1];
                col.Add(Temptext);
            }

            txt_InformationNeeds.DataSource = col;
            txt_InformationNeeds.AutoCompleteSource = AutoCompleteSource.ListItems;
            txt_InformationNeeds.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txt_InformationNeeds.Text = "";
        }

        // Paging...
        int total = 0;
        int skip = 0;

        // Saving...
        int TopicID = 1;
        List<Collection> ResultCollectionList = new List<Collection>();

        #region Step 1: Indexing
        private void btn_CollectionPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Please Choose Your Path of Collection";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string sSelectedPath = fbd.SelectedPath;
                txt_CollectionPath.Text = sSelectedPath;
            }
        }

        private void btn_IndexPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Please Choose Your Path for Indexing";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string sSelectedPath = fbd.SelectedPath;
                txt_IndexPath.Text = sSelectedPath;
            }
        }

        private void btn_GenerateIndex_Click(object sender, EventArgs e)
        {
            // Check path
            if (String.IsNullOrEmpty(txt_CollectionPath.Text))
            {
                MessageBox.Show("Please Set the path of collection first!");
                return;
            }
            if (String.IsNullOrEmpty(txt_IndexPath.Text))
            {
                MessageBox.Show("Please Set the path of index first!");
                return;
            }


            // Do the index here.
            if (!generator.IsDirectoryEmpty(txt_IndexPath.Text))
            {
                if (MessageBox.Show("The directory is not empty, do you want to delete all the files inside? (Yes-Delete All, No-Select Again)",
                        "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    generator.DeleteFiles(txt_IndexPath.Text);
                    Indexing(generator);
                }
            }
            else
            {
                Indexing(generator);
            }

            if (generator.IsIndexing == true)
            {
                lblSearch.Visible = true;
            }
        }

        private void Indexing( IndexGenerator generator)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            generator.StartIndex(txt_IndexPath.Text, txt_CollectionPath.Text);
            stopWatch.Stop();
            long ts = stopWatch.ElapsedMilliseconds;
            lbl_IndexingTime.Text = ts.ToString() + " ms";
        }

        #endregion

        #region Step 2: Searching
        private void Type_Click(object sender, EventArgs e)
        {
            if (ddl_Type.SelectedItem != "Single Term")
            {
                ddl_Fields.Visible = false;
            }
            else {
                ddl_Fields.Visible = true;
            }

        }
        private void lblSearch_Click(object sender, EventArgs e)
        {
            //Check the saving path
            if (String.IsNullOrEmpty(txt_Saving.Text))
            {
                MessageBox.Show("Please choose a path to save results.");
                return;
            }
            if (String.IsNullOrEmpty(txt_FileName.Text))
            {
                MessageBox.Show("Please input a file name.");
                return;
            }
            // Check is existing or new file.
            String filePath = String.Format("{0}\\{1}.txt", txt_Saving.Text, txt_FileName.Text);
            if (File.Exists(filePath))
            {
                if (MessageBox.Show("The file is existing, do you want to append text into the file? (Yes: append it, No: Set a new file name.)",
                        "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // User wanna append it, so get the Topic ID from document...
                    var lastLine = File.ReadLines(filePath).Count() != 0 ? File.ReadLines(filePath).Last() : "000";
                    if (!String.IsNullOrEmpty(lastLine))
                    {
                        var firstword = lastLine.ToString().Split(' ')[0];
                        try
                        {
                            TopicID = Convert.ToInt32(firstword) + 1;
                        }
                        catch (Exception ex) {
                            MessageBox.Show("Fail appending! The format in the file is wrong. Please set a new file name.");
                            return;
                        }
                    }
                }
                else {
                    return;
                }
            }

            btn_SaveBrowse.Visible = false;
            txt_Saving.ReadOnly = true;
            txt_FileName.ReadOnly = true;

            // Calculate the Process Time
            Lucene.Net.Analysis.Analyzer analyzer;
            if (ckb_Analzer.Checked)
            {
                analyzer = new Lucene.Net.Analysis.SimpleAnalyzer();
            }
            else {
                analyzer = generator.analyzer;
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Searcher searcher = new Searcher(txt_IndexPath.Text, analyzer, generator.writer);
            searcher.CreateSearcher();
            searcher.CreateParser(ddl_Type.SelectedItem.ToString(), ddl_Fields.SelectedItem.ToString());

            // Searching and generate result
            Dictionary<string, float> resultList = searcher.DisplayResults(searcher.SearchIndex(txt_InformationNeeds.Text), generator.collectionList);
            lbl_QueryText.Text = searcher.query.ToString();
            ResultCollectionList =
                (from c in generator.collectionList
                 join r in resultList on c.DocID equals r.Key
                 select new Collection
                 {
                     DocID = c.DocID,
                     Title = c.Title,
                     Author = c.Author,
                     Bibliographic = c.Bibliographic,
                     Words = c.Words,
                     Score = r.Value
                 }).OrderByDescending(n => n.Score).ToList();
            stopWatch.Stop();
            long ts = stopWatch.ElapsedMilliseconds;
            lbl_SearchingTime.Text = ts.ToString() + " ms";

            // Set columns to listview
            GenerateListView();
            // Set the total number of documents
            total = resultList.Count();
            lblTotalDocs.Text = total + " docs";

            // Saving
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(filePath, true))
            {
                int rank = 1;
                foreach (Collection c in ResultCollectionList)
                {
                    string TopicIDString = string.Format("{0:000}", TopicID);
                    file.NewLine = "\n";
                    file.WriteLine(String.Format("{0} {1} {2} {3} {4} {5}", TopicIDString, "Q0", c.DocID, rank.ToString(), c.Score, "9913661_9913351_10032711_RetrievalHero"));
                    rank = rank + 1;
                }
                TopicID++;
            }
        }

        private void GenerateListView()
        {
            dv_result.Columns.Clear();
            //dv_result.View = View.Details;
            dv_result.Columns.Add("DocID", "DocID");
            dv_result.Columns.Add("Title", "Title");
            dv_result.Columns.Add("Author", "Author");
            dv_result.Columns.Add("Bibliographic", "Bibliographic");
            dv_result.Columns.Add("Abtract", "Abtract");
            DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
            dv_result.Columns.Add(btn);
            btn.HeaderText = "Detail";
            btn.Text = "Detail";
            btn.Name = "btn";
            btn.UseColumnTextForButtonValue = true;

            // Add items to listview
            foreach (Collection c in ResultCollectionList.Skip(skip).Take(10))
            {
                string abtractFirstSentence = c.Words.Split('.')[0];
                //string[] row = { c.DocID, c.Title, c.Author, c.Bibliographic, string.IsNullOrEmpty(abtractFirstSentence) ? string.Empty : abtractFirstSentence.TrimEnd() + "." };
                //var listViewItem = new ListViewItem(row);
                
                //Create the new row first and get the index of the new row
                int rowIndex = this.dv_result.Rows.Add();

                //Obtain a reference to the newly created DataGridViewRow 
                var newRow = this.dv_result.Rows[rowIndex];

                //Now this won't fail since the row and columns exist 
                newRow.Cells["DocID"].Value = c.DocID;
                newRow.Cells["Title"].Value = c.Title;
                newRow.Cells["Author"].Value = c.Author;
                newRow.Cells["Bibliographic"].Value = c.Bibliographic;
                newRow.Cells["Abtract"].Value = string.IsNullOrEmpty(abtractFirstSentence) ? string.Empty : abtractFirstSentence.TrimEnd() + ".";
            }

            btn_Previous.Visible = true;
            btn_Next.Visible = true;
        }

        private void btn_Previous_Click(object sender, EventArgs e)
        {
            // if skip >= 10, means the user is not in the first page! because it still can minus 10;
            if (skip >= 10)
            {
                skip = skip - 10;
                GenerateListView();
            }
            else
            {
                MessageBox.Show("You are in the first page!");
            }
        }

        private void btn_Next_Click(object sender, EventArgs e)
        {
            // if skip + 10 > total number of docs, means the user is in the last page!
            if (skip + 10 > total)
            {
                MessageBox.Show("You are in the last page!");
            }
            else
            {
                skip = skip + 10;
                GenerateListView();
            }
        }

        private void btn_SaveBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Please choose a path to save results";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string sSelectedPath = fbd.SelectedPath;
                txt_Saving.Text = sSelectedPath;
            }
        }

        private void dv_result_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
                e.RowIndex >= 0)
            {
                //TODO - Button Clicked - Execute Code Here
                string FullAbtract = ResultCollectionList.Where(n => n.DocID == dv_result.Rows[e.RowIndex].Cells[0].Value.ToString()).Select( n => n.Words).FirstOrDefault();
                DetailForm form = new DetailForm(FullAbtract);
                form.Show();
            }
        }
        #endregion

        private void txt_InformationNeeds_TextChanged(object sender, EventArgs e)
        {
          
        }
    }
}
