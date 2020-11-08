/*
 * Jarod Guerrero
 * CSS FourThreeSix A
 * 11/17/19
 * 
 * Web Storage Application
 * https://program4webstorage.azurewebsites.net/About
 * 
 * Class representing an entity from the input.txt file.
 * Inherits the TableEntity class allowing it to be programatically
 * added to Azure Table storage.
 */
using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

public class InputEntity : TableEntity
{
    /*
     * Main constructor. Create an entity containing a first name, last name,
     * and variable number of attributes.
     * Last name and first name are chosen are the primary key and sort key respectively,
     * since the combination is guaranteed to be unique. In a larger-scale application,
     * personID would be a better fit.
     * Entities can be added to Azure Table Storage
     */
	public InputEntity(string last, string first, string attributeString)
	{
        this.PartitionKey = last;
        this.RowKey = first;

        if(attributeString == "")
        {
            attributes = null;
        } 
        else
        {
            attributes = attributeString;
        }

        // ---
        // Testing code to dynamically add fields to a table
        // ---
        // attributeString.Equals("") ? attributes = "" : attributes = attributeString;
        /* Decided to treat the attributes as an attribute string.
         * Not sure if I'd be able to add a list of tuples
        // Parse the attribute string
        string[] attributeArr = attributes.Split(' ');
        foreach(var entry in attributeArr)
        {
            // Split by the equal's sign
            string[] attributeParts = entry.Split('=');
            attributes.Add(Tuple<string, string>(attributeParts[0], attributeParts[1]));
        }
        // Will this add cleanly to the NoSQL db?
        // Maybe need to track the unique entries so they'll
        // be fields in the db.
        */
	}

    /*
     * Constructor taking both a first and last name. Used for entity deletion
     * and querying based on primary and row keys.
     */
    public InputEntity(string last, string first)
    {
        this.PartitionKey = last;
        this.RowKey = first;
        this.ETag = "*";
    }

    // Default constructor.
    // Empty first/last/attributes
    public InputEntity()
    {
        attributes = "";
    }
    //public List<Tuble<string, string>> attributes; // Contains a variable number of attributes
    public string attributes { get; set; }
}
