/*
 * Jarod Guerrero
 * CSS 436 A
 * 11/17/19
 * 
 * Program 4 - Web Storage Application
 * https://program4webstorage.azurewebsites.net/About
 * 
 * Code used to add functionality to the website UI.
 * Accomplishes load, clear, and query operations using
 * three buttons.
 */

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace Program4WebStorage
{
    public partial class About : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Should I store the connections here? State can't be kept on the server,
            // since that wouldn't be RESTful, but a session containing connections might
            // be useful on the client side.

            // Check if data is currently stored in the table. If so, update the
            // button so it can be used to query.
            String storageconn = System.Configuration.ConfigurationManager.AppSettings.Get("StorageConnectionString");
            CloudStorageAccount storageacc = CloudStorageAccount.Parse(storageconn);
            CloudBlobClient blobClient = storageacc.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("p4container");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("input.txt");
            Button3.Enabled = blockBlob.Exists();
        }

        /*
         * Load Data Button:
         * Loads data from the azure URL provided in the program instructions.
         * Populates an azure table storage with entries parsed from the input file
         * containing a last name, first name, and variable number of attributes
         */
        protected void Button1_Click(object sender, EventArgs e)
        {
            const string DataURL = "http://css490.blob.core.windows.net/lab4/input.txt";

            // Establish connection to the specified URL and store it as
            // an object in blob storage
            // Flow: Account -> Container -> Blob (actual file)
            Label1.Text = "Connecting to Storage Account";
            String storageconn = System.Configuration.ConfigurationManager.AppSettings.Get("StorageConnectionString");
            CloudStorageAccount storageacc = CloudStorageAccount.Parse(storageconn);

            CloudBlobClient blobClient = storageacc.CreateCloudBlobClient();
            // Create container "P4Container" if none exists
            CloudBlobContainer container = blobClient.GetContainerReference("p4container");
            container.CreateIfNotExists();

            CloudBlockBlob blockBlob = container.GetBlockBlobReference("input.txt");

            CloudTableClient tableClient = storageacc.CreateCloudTableClient();
            CloudTable inputTable = tableClient.GetTableReference("p4inputTable");
            DeleteAllTableEntities(ref blockBlob, ref inputTable);
            
            Label1.Text = "Uploading Input File";
            // Upload file from DataURL to a blob in the P4Container

            var req = System.Net.WebRequest.Create(DataURL);
            using (Stream filestream = req.GetResponse().GetResponseStream())
            {
                blockBlob.UploadFromStream(filestream);
            }
            Label1.Text = "Blob Successfully Uploaded";
            // Running it again would simply update it, since the same container is reused

            //--------------
            // Next Step, parse the file into Azure Table entries


            // Delete table entries if previous data was already stored.
            inputTable.CreateIfNotExists();

            // Create a class that defines the properties of each entity
            // First name, last name, variable number of properties (vector?)

            // Get the input file text from blockBlob reference
            string inputText = blockBlob.DownloadText();

            using (StringReader reader = new StringReader(inputText))
            {
                // Load all entries into an insert operation. Can't use a batch operation
                // since the primary key (last name) isn't guaranteed to be the same
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    // Split string. First and last name are stored in the first two elements.
                    // Everything else is an attribute
                    char[] delimiters = { ' ' };
                    string[] info = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    string attributes = "";

                    for (int i = 2; i < info.Length; i++)
                    {
                        attributes += info[i] + " ";
                    }
                    InputEntity newEntry = new InputEntity(info[0], info[1], attributes);
                    TableOperation insertOperation = TableOperation.Insert(newEntry);
                    inputTable.ExecuteAsync(insertOperation);
                }
                // Push INSERT operation
                //inputTable.ExecuteBatchAsync(batchOperation);

                Label1.Text = "Successfully added input to table";
                Button3.Enabled = true;
                Label2.Text = "";
            }
        }

        /*
         * Clear button: Deletes the input.txt blob file from object storage and
         * removes all entries from the table.
         * 
         * NOTE:    Deleting the table is a possible approach, but there's a delay between
         *          table deletion and when we can re-create the table, resulting in
         *          a server conflict response 409.
         * 
         * What would happen if they pressed clear without uploading anything?
         * Maybe some of the objects would be null
         */
        protected void Button2_Click(object sender, EventArgs e)
        {
            // Disable the query button
            Button3.Enabled = false;

            String storageconn = System.Configuration.ConfigurationManager.AppSettings.Get("StorageConnectionString");
            CloudStorageAccount storageacc = CloudStorageAccount.Parse(storageconn);
            CloudBlobClient blobClient = storageacc.CreateCloudBlobClient();
            
            // Delete blob object
            CloudBlobContainer container = blobClient.GetContainerReference("p4container");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("input.txt");

            CloudTableClient tableClient = storageacc.CreateCloudTableClient();
            CloudTable inputTable = tableClient.GetTableReference("p4inputTable");

            DeleteAllTableEntities(ref blockBlob, ref inputTable);
            Label2.Text = "Cleared table entities";
            blockBlob.DeleteIfExists();
            Label2.Text = "Successfully deleted blob and table from storage";
            Label1.Text = "";
        }

        /*
         * Query button:
         * Queries the currently stored table for a last name and first name.
         * Displays the names of matching results and their attributes.
         * 
         * Precondition:    The input file and corresponding table must already
         *                  be stored. Query button is disabled otherwise
         */
        protected void Button3_Click(object sender, EventArgs e)
        {
            String storageconn = System.Configuration.ConfigurationManager.AppSettings.Get("StorageConnectionString");
            CloudStorageAccount storageacc = CloudStorageAccount.Parse(storageconn);
            CloudBlobClient blobClient = storageacc.CreateCloudBlobClient();
            CloudTableClient tableClient = storageacc.CreateCloudTableClient();
            CloudTable inputTable = tableClient.GetTableReference("p4inputTable");

            // Case 1: Partition and row key provided (last and first name)
            if (TextBox1.Text != "" && TextBox2.Text != "")
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<InputEntity>(TextBox1.Text, TextBox2.Text);
                TableResult retrievedResult = inputTable.Execute(retrieveOperation);
                if (retrievedResult.Result != null)
                {
                    TableRow r = new TableRow();
                    TableCell cLast = new TableCell();
                    TableCell cFirst = new TableCell();
                    TableCell cAtr = new TableCell();

                    cLast.Text = ((InputEntity)retrievedResult.Result).PartitionKey;
                    cFirst.Text = ((InputEntity)retrievedResult.Result).RowKey;
                    cAtr.Text = ((InputEntity)retrievedResult.Result).attributes;

                    r.Cells.Add(cLast);
                    r.Cells.Add(cFirst);
                    r.Cells.Add(cAtr);

                    Table1.Rows.Add(r);
                } 
                else
                {
                        // Output empty row
                        TableRow r = new TableRow();
                        TableCell cEmpt = new TableCell();
                        cEmpt.Text = "No results found";
                        r.Cells.Add(cEmpt);
                        Table1.Rows.Add(r);
                }
            }
            // Case 2: Only partition key provided (last name)
            // Can return multiple results
            else if(TextBox1.Text != "")
            {
                // Code used from:
                // https://docs.microsoft.com/en-us/azure/visual-studio/vs-storage-aspnet5-getting-started-
                TableQuery<InputEntity> query = new TableQuery<InputEntity>().Where
                    (TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, TextBox1.Text));
                TableContinuationToken token = null;
                int resultCount = 0;
                do
                {
                    TableQuerySegment<InputEntity> resultSegment = inputTable.ExecuteQuerySegmented(query, token);
                    token = resultSegment.ContinuationToken;

                    foreach (InputEntity entity in resultSegment.Results)
                    {
                        resultCount++;
                        TableRow r = new TableRow();
                        TableCell cLast = new TableCell();
                        TableCell cFirst = new TableCell();
                        TableCell cAtr = new TableCell();

                        cLast.Text = entity.PartitionKey;
                        cFirst.Text = entity.RowKey;
                        cAtr.Text = entity.attributes;

                        r.Cells.Add(cLast);
                        r.Cells.Add(cFirst);
                        r.Cells.Add(cAtr);

                        Table1.Rows.Add(r);
                    }
                } while (token != null);

                if (resultCount == 0) // No results found
                {
                    // Output empty row
                    TableRow r = new TableRow();
                    TableCell cEmpt = new TableCell();
                    cEmpt.Text = "No results found";
                    r.Cells.Add(cEmpt);
                    Table1.Rows.Add(r);
                }
            }

            // Case 3: Only row key provided (first name)
            else
            {
                TableQuery<InputEntity> query = new TableQuery<InputEntity>().Where
                    (TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, TextBox2.Text));
                TableContinuationToken token = null;

                int resultCount = 0;
                do
                {
                    TableQuerySegment<InputEntity> resultSegment = inputTable.ExecuteQuerySegmented(query, token);
                    token = resultSegment.ContinuationToken;

                    foreach (InputEntity entity in resultSegment.Results)
                    {
                        resultCount++;

                        TableRow r = new TableRow();
                        TableCell cLast = new TableCell();
                        TableCell cFirst = new TableCell();
                        TableCell cAtr = new TableCell();

                        cLast.Text = entity.PartitionKey;
                        cFirst.Text = entity.RowKey;
                        cAtr.Text = entity.attributes;

                        r.Cells.Add(cLast);
                        r.Cells.Add(cFirst);
                        r.Cells.Add(cAtr);

                        Table1.Rows.Add(r);
                    }
                } while (token != null);

                if(resultCount == 0) // No results found
                {
                    // Output empty row
                    TableRow r = new TableRow();
                    TableCell cEmpt = new TableCell();
                    cEmpt.Text = "No results found";
                    r.Cells.Add(cEmpt);
                    Table1.Rows.Add(r);
                }
            }
        }
        /*
         * Deletes all entities from a table. The table is left empty, but is 
         * still preserved in storage. Uses the entries from input.txt in blob
         * storage to perform the deletions. Requires input.txt to still be in 
         * blob storage.
         * 
         * Precondition:    input.txt is in blob storage and was used to generate
         *                  the current table also in storage
         */
        private void DeleteAllTableEntities(ref CloudBlockBlob blockBlob, ref CloudTable inputTable)
        {
            if (blockBlob.Exists() && inputTable.Exists())
            { // Get primary and row key from input.txt
                string inputText = blockBlob.DownloadText();
                using (StringReader reader = new StringReader(inputText))
                {
                    // Load all entries into an insert operation. Can't use a batch operation
                    // since the primary key (last name) isn't guaranteed to be the same
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Split string. First and last name are stored in the first two elements.
                        // Everything else is an attribute
                        char[] delimiters = { ' ' };
                        string[] info = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                        InputEntity entityToDelete = new InputEntity(info[0], info[1]);
                        TableOperation deleteOperation = TableOperation.Delete(entityToDelete);
                        inputTable.ExecuteAsync(deleteOperation);
                    }
                }
            }
        }
    }
}