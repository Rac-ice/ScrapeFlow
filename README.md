A simple single-page scraping program.

Task Type: Navigate Fill Click Wait Table

Selector: You can use either CSS selectors or XPath. When using XPath, you must prefix it with xpath=.

TableAction Class
Selector: ""
Columns: [] # The index of the table to scrape.
SaveToDb: true or false
Table: {
    Name: "TableName"
    Columns: [ # Column information of the database table.
        {
            Name: ""
            Type: "" # Types in C#
            IsPrimaryKey: true or false
            IsIdentity: true or false
            Length: 
        }
    ]
}

Columns.Count == Table.Columns.Count (No primary key)

Columns.Count == Table.Columns.Count - 1 (Has a primary key)
