using System;
using System.Collections.Generic;
using System.Text;

namespace MappeVacciniG4.CLASSES
{
    // Regions / province
    public class Rootobject
    {
        public string type { get; set; }
        public float[] bbox { get; set; }
        public Feature[] features { get; set; }
    }

    public class Feature
    {
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public object[][][] coordinates { get; set; }
    }

    public class Properties
    {
        public string reg_name { get; set; }
        public int reg_istat_code_num { get; set; }
        public string reg_istat_code { get; set; }
    }

    // Pins Json
    public class PinsJson
    {
        public Schema schema { get; set; }
        public Datum[] data { get; set; }
    }

    public class Schema
    {
        public Field[] fields { get; set; }
        public string[] primaryKey { get; set; }
        public string pandas_version { get; set; }
    }

    public class Field
    {
        public string name { get; set; }
        public string type { get; set; }
    }

    public class Datum
    {
        public int index { get; set; }
        public string area { get; set; }
        public string provincia { get; set; }
        public string comune { get; set; }
        public string presidio_ospedaliero { get; set; }
        public string codice_NUTS1 { get; set; }
        public string codice_NUTS2 { get; set; }
        public int codice_regione_ISTAT { get; set; }
        public string nome_area { get; set; }
    }

    // VacciniSummary latest
    public class RootobjectVacciniSummary
    {
        public SchemaVacciniSummary schema { get; set; }
        public DatumVacciniSummary[] data { get; set; }
    }

    public class SchemaVacciniSummary
    {
        public FieldVacciniSummary[] fields { get; set; }
        public string[] primaryKey { get; set; }
        public string pandas_version { get; set; }
    }

    public class FieldVacciniSummary
    {
        public string name { get; set; }
        public string type { get; set; }
    }

    public class DatumVacciniSummary
    {
        public int index { get; set; }
        public string area { get; set; }
        public int dosi_somministrate { get; set; }
        public int dosi_consegnate { get; set; }
        public float percentuale_somministrazione { get; set; }
        public DateTime ultimo_aggiornamento { get; set; }
        public string codice_NUTS1 { get; set; }
        public string codice_NUTS2 { get; set; }
        public int codice_regione_ISTAT { get; set; }
        public string nome_area { get; set; }
    }

    // Restrizioni
    public class Restrizioni
    {
        public string[] bianca { get; set; }
        public string[] gialla { get; set; }
        public string[] arancione { get; set; }
        public string[] rossa { get; set; }
    }



    class JsonClass
    {
    }
}
