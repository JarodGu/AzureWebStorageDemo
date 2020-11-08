# AzureWebStorageDemo

![Program Demo](https://www.dropbox.com/s/u603u30zeenei1b/CSS436%20P4%20Demo.png?raw=1)

## Abstract
This web app makes use of Azure services to accomplish three main tasks: loading data from a static URL into object storage, parsing it into a stored table, and making queries. Three buttons are used to load and parse data into object and table storage, delete current data, and query for entities based on either/both a last and first name.

## Design
I decided to go with Azure and ASP.NET webforms since I worked with it in class and was the recommended approach. Azure object storage is like AWS’s but with different terminology. Objects are called blobs and buckets are containers. My main code contributions are in About.aspx.cs and InputEntity.cs.

![Program Cloud Architecture](https://www.dropbox.com/s/0b94q6x6e5tnzub/CSS436%20P4%20Diagram.png?raw=1)

Parsing the input file into table storage was challenging. Each entry in the input file has a last name, first name, and variable number of attributes. Entries are called entities in Azure Table Storage. I utilized an InputEntity class that inherited the TableEntity class to programmatically add them into storage. I chose the last name and first name for the partition key and row key respectively. A combination of both is guaranteed to be unique. I also considered dynamically adding fields based on new attributes but decided to store all attributes in a single string for simplicity.

There are three possible combinations for querying the table based on a last name and first name. Either or both may be provided. It’s a bit more difficult querying a NoSQL database programmatically compared to SQL. Microsoft’s table storage documentation greatly helped with writing code [1]. Documentation including methods and objects was difficult to find since it was merged with CosmosDB’s.

Deletion also wasn’t as simple as expected. According to a Stack Overflow post, deleting an entire table from storage makes it temporarily unavailable for being recreated [2]. I received a conflict error code 409 after clicking the clear data and load data buttons in quick sequence. I decided to delete all entries in the table instead, allowing the clear and load data buttons to be pressed repeatedly. It parses the currently stored input file for partition-row key combinations and deletes them.

## Scalability and Monitoring
State is not stored on the client side after pressing a button. Load data, clear data, and querying can all be performed separately, allowing one to resume their web session at any time. Two factor that affects scalability are not using asynchronous operations and deletion. Most cloud object functions have an Async suffix and using these allows functions to continue without waiting for an operation to finish. Deleting and loading data may also encounter errors when used on multiple clients simultaneously. Clearing the table data is dependent on the text file in blob storage, and a misalignment would allow “ghost” entries to persist.

Monitoring is limited since the web app uses the F1 free tier package including 1GB of storage, 1GB of memory, and 60 minutes of compute. I could check the size of the input file to make sure it doesn’t exceed maybe 900MB. Azure includes an alerts and metrics page that I can use to notify me when resource utilization reaches a certain capacity.

## Service Level Agreement (SLA) Estimation
The primary Azure services used are App Service and Storage. App Service uses the F1 App Service plan, which has no guaranteed SLA [3]. Storage is in USWest2 located in Washington state and uses locally redundant storage (LRS) with a read availability of 99.9% [4]. If we used the minimum tier for an app service plan SLA, our total availability would be:

```Latex
99.95% App Service Availability * 99.9% LRS Storage Availability
= 99.85% Total Availability
```

## Final Thoughts
Program 4 encompasses the web stack needed to create and store a database in the cloud. ASP.NET web forms allowed me to focus on functionality by starting with a template site and using simple control objects like labels, textboxes, and buttons. Improvements in a future revision could include using asynchronous operations to improve responsiveness, de-coupling dependency on the input blob for table deletion, removing unused web pages, and parsing attributes to dynamically add more fields to the table. 

## Sources
1. https://docs.microsoft.com/bs-latn-ba/azure/visual-studio/vs-storage-aspnet5-getting-started-tables?toc=/aspnet/core/toc.json&bc=/aspnet/core/breadcrumb/toc.json&view=aspnetcore-3.0
2. https://stackoverflow.com/questions/26326413/delete-all-azure-table-records
3. https://azure.microsoft.com/en-us/support/legal/sla/app-service/v1_4/
4. https://azure.microsoft.com/en-us/support/legal/sla/storage/v1_5/
