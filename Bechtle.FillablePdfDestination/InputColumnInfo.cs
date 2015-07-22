namespace Bechtle.FillablePdfDestination
{
    using Microsoft.SqlServer.Dts.Runtime.Wrapper;

    public struct InputColumnInfo
    {
        private string columnName;

        private DataType dataType;

        private int lineageID;

        private string identificationString;

        //ToDo Properties vereinheitlichen (nur get)
        
        public InputColumnInfo(string columnName, DataType dataType, int lineageID, string identificationString)
        {
            this.columnName = columnName;
            this.dataType = dataType;
            this.lineageID = lineageID;
            this.identificationString = identificationString;
        }
        
        public string ColumnName
        {
            get
            {
                return this.columnName;
            }
            set
            {
                this.columnName = value;
            }
        }

        public DataType DataType
        {
            get
            {
                return this.dataType;
            }
            set
            {
                this.dataType = value;
            }
        }

        public int LineageId
        {
            get
            {
                return this.lineageID;
            }
            set
            {
                this.lineageID = value;
            }
        }

        public string IdentificationString
        {
            get
            {
                return identificationString;
            }
            set
            {
                identificationString = value;
            }
        }
    }
}