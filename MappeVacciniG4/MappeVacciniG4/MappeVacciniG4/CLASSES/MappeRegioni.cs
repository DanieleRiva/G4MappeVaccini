using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MappeVacciniG4.CLASSES
{
    // Main execution
    public class MappeRegioni
    {
        string[] regions = new string[] { "Piemonte", "Valle d'Aosta", "Lombardia", "Trentino", "Veneto", "Friuli", "Liguria", "Emilia-Romagna", "Toscana", "Umbria", "Marche", "Lazio", "Abruzzo", "Molise", "Campania", "Puglia", "Basilicata", "Calabria", "Sicilia", "Sardegna" };
        public float[] popolazione = { 4356000, 125666, 10060000, 1072000, 4906000, 1215000, 1551000, 4459000, 3730000, 882015, 1525000, 5879000, 1312000, 305617, 5802000, 4029000, 562869, 1947000, 5000000, 1640000 };
        static readonly HttpClient client = new HttpClient();
        static string vacciniSummaryGet = "https://raw.githubusercontent.com/italia/covid19-opendata-vaccini/master/dati/vaccini-summary-latest.json";
        static string restrizioniGet = "https://covid19.zappi.me/coloreRegioni.php";
        static string covidGet = "https://raw.githubusercontent.com/pcm-dpc/COVID-19/master/dati-json/dpc-covid19-ita-regioni-latest.json";
        List<float> listaGradazioni = new List<float>();

        RootobjectVacciniSummary JsonColors = new RootobjectVacciniSummary();
        Restrizioni JsonRestrizioni = new Restrizioni();
        RootobjectCovid JsonCovid = new RootobjectCovid();
        float c = 0;
        string restrizioniDati = string.Empty;
        string covidDati = string.Empty;

        public async Task<Polygon[]> InitData(int tipologia)
        {
            Rootobject res = new Rootobject();
            float massimo = 0;
            float minimo = 0;
            float salto = 0;
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(EmptyClass)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("MappeVacciniG4.GEOJSON.regioni2.geojson");

            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                res = JsonConvert.DeserializeObject<Rootobject>(json);
            }

            Polygon[] regionsPoly = new Polygon[20];

            double valore1 = 0;
            double valore2 = 0;

            if (tipologia == 0)
            {
                string dati = await client.GetStringAsync(vacciniSummaryGet);
                JsonColors = JsonConvert.DeserializeObject<RootobjectVacciniSummary>(dati);
            }
            else if (tipologia == 1)
            {
                string covidDati = "{ \n\"schema\":{\n\"fields\":[\n{\n\"name\":\"data\",\n\"type\":\"datetime\" \n}, \n{ \n\"name\":\"stato\", \n\"type\":\"string\" \n}, \n{ \n\"name\":\"codice_regione\", \n\"type\":\"integer\" \n}, \n{ \n\"name\":\"denominazione_regione\", \n\"type\":\"string\" \n}, \n{ \n\"name\":\"lat\", \n\"type\":\"float\" \n}, \n{ \n\"name\":\"long\", \n\"type\":\"float\" \n}, \n{ \n\"name\":\"ricoverati_con_sintomi\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"terapia_intensiva\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"totale_ospedalizzati\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"isolamento_domiciliare\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"totale_positivi\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"variazione_totale_positivi\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"nuovi_positivi\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"dimessi_guariti\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"deceduti\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"casi_da_sospetto_diagnostico\", \n\"type\":\"object\" \n}, \n{ \n\"name\":\"casi_da_screening\", \n\"type\":\"object\" \n}, \n{ \n\"name\":\"totale_casi\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"tamponi\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"casi_testati\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"note\", \n\"type\":\"string\" \n}, \n{ \n\"name\":\"ingressi_terapia_intensiva\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"note_test\", \n\"type\":\"object\" }, \n{ \n\"name\":\"note_casi\", \n\"type\":\"string\" \n}, \n{ \n\"name\":\"totale_positivi_test_molecolare\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"totale_positivi_test_antigenico_rapido\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"tamponi_test_molecolare\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"tamponi_test_antigenico_rapido\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"codice_nuts_1\", \n\"type\":\"string\" \n}, \n{ \n\"name\":\"codice_nuts_2\", \n\"type\":\"string\" \n}, \n], \n}, \n\"data\":";
                covidDati += await client.GetStringAsync(covidGet);
                covidDati += "}";
                Debug.WriteLine(covidDati);
                JsonCovid = JsonConvert.DeserializeObject<RootobjectCovid>(covidDati);
            }
            else if (tipologia == 2)
            {
                restrizioniDati = await client.GetStringAsync(restrizioniGet);
                JsonRestrizioni = JsonConvert.DeserializeObject<Restrizioni>(restrizioniDati);
            }

            if (tipologia == 0) // Find Maximum and minimum persentage of vaccines
            {
                listaGradazioni.Clear();

                for (int i = 0; i < 20; i++)
                {
                    int coloreTrentino = 0;

                    foreach (var region in JsonColors.data)
                    {
                        if (region.nome_area == res.features[i].properties.reg_name || res.features[i].properties.reg_name == "Valle d'Aosta/Vallée d'Aoste" && region.index == 19 || res.features[i].properties.reg_name == "Trentino-Alto Adige/Südtirol" && region.index == 11 || res.features[i].properties.reg_name == "Trentino-Alto Adige/Südtirol" && region.index == 12)
                        {
                            if (region.index == 11)
                                coloreTrentino += region.dosi_somministrate;
                            else if (region.index == 12)
                            {
                                coloreTrentino += region.dosi_somministrate;
                                c = (coloreTrentino / (popolazione[i])) * 100;
                                listaGradazioni.Add(c);
                            }
                            else
                            {
                                c = (region.dosi_somministrate / (popolazione[i])) * 100;
                                listaGradazioni.Add(c);
                            }

                            if (c > massimo)
                                massimo = c;

                            if (c < minimo || region.nome_area == "Piemonte")
                                minimo = c;
                        }
                    }

                    salto = massimo - minimo;
                    salto /= 14;
                }
            }
            else if (tipologia == 1) // Find Maximum and minimum persentage of covid
            {
                listaGradazioni.Clear();

                for (int i = 0; i < 20; i++)
                {
                    int coloreTrentino = 0;

                    foreach (var region in JsonCovid.data)
                    {
                        if (i == 5 && region.denominazione_regione == "Friuli Venezia Giulia")
                        {

                        }

                        if (region.denominazione_regione == res.features[i].properties.reg_name || res.features[i].properties.reg_name == "Valle d'Aosta/Vallée d'Aoste" && region.codice_regione == 2 || res.features[i].properties.reg_name == "Trentino-Alto Adige/Südtirol" && region.codice_regione == 21 || res.features[i].properties.reg_name == "Trentino-Alto Adige/Südtirol" && region.codice_regione == 22 || res.features[i].properties.reg_name == "Friuli-Venezia Giulia" && region.denominazione_regione == "Friuli Venezia Giulia")
                        {
                            if (region.codice_regione == 21)
                            {
                                coloreTrentino += region.totale_positivi;
                            }
                            else if (region.codice_regione == 22)
                            {
                                coloreTrentino += region.totale_positivi;
                                c = (coloreTrentino / (popolazione[i])) * 100;
                                listaGradazioni.Add(c);
                                Debug.WriteLine($"Aggiunto dato della regione {region.denominazione_regione}");
                            }
                            else
                            {
                                c = (region.totale_positivi / (popolazione[i])) * 100;
                                listaGradazioni.Add(c);
                                Debug.WriteLine($"Aggiunto dato della regione {region.denominazione_regione}");
                            }

                            if (c > massimo)
                                massimo = c;

                            if (c < minimo || region.denominazione_regione == "Piemonte")
                                minimo = c;
                        }
                    }

                    salto = massimo - minimo;
                    salto /= 14;
                }
            }

            for (int i = 0; i < 20; i++) // Create polygons scrolling throught regions
            {
                Polygon polygon = new Polygon();
                polygon.StrokeWidth = 5;
                polygon.StrokeColor = Color.DarkSlateGray;

                if (tipologia == 0)
                    foreach (var region in JsonColors.data) // Colors based on vaccines
                    {
                        if (region.nome_area == res.features[i].properties.reg_name || res.features[i].properties.reg_name == "Valle d'Aosta/Vallée d'Aoste" && region.index == 19 || res.features[i].properties.reg_name == "Trentino-Alto Adige/Südtirol" && region.index == 11 || res.features[i].properties.reg_name == "Trentino-Alto Adige/Südtirol" && region.index == 12)
                        {
                            if (region.index != 11)
                            {
                                c = listaGradazioni.ElementAt(i); // Take persentage of current region from the list

                                if (c < minimo + salto)
                                    polygon.FillColor = Color.FromHex("#243B6EFF"); // 14%
                                else if (c < minimo + (2 * salto))
                                    polygon.FillColor = Color.FromHex("#263B6EFF"); // 15%
                                else if (c < minimo + (3 * salto))
                                    polygon.FillColor = Color.FromHex("#333B6EFF"); // 20%
                                else if (c < minimo + (4 * salto))
                                    polygon.FillColor = Color.FromHex("#403B6EFF"); // 25%
                                else if (c < minimo + (5 * salto))
                                    polygon.FillColor = Color.FromHex("#4D3B6EFF"); // 30%
                                else if (c < minimo + (6 * salto))
                                    polygon.FillColor = Color.FromHex("#593B6EFF"); // 35%
                                else if (c < minimo + (7 * salto))
                                    polygon.FillColor = Color.FromHex("#4D3B6EFF"); // 40%
                                else if (c < minimo + (8 * salto))
                                    polygon.FillColor = Color.FromHex("#4D3B6EFF"); // 45%
                                else if (c < minimo + (9 * salto))
                                    polygon.FillColor = Color.FromHex("#803B6EFF"); // 50%
                                else if (c < minimo + (10 * salto))
                                    polygon.FillColor = Color.FromHex("#8C3B6EFF"); // 55%
                                else if (c < minimo + (11 * salto))
                                    polygon.FillColor = Color.FromHex("#993B6EFF"); // 60%
                                else if (c < minimo + (12 * salto))
                                    polygon.FillColor = Color.FromHex("#A63B6EFF"); // 65%
                                else if (c < minimo + (13 * salto))
                                    polygon.FillColor = Color.FromHex("#B33B6EFF"); // 70%
                                else
                                    polygon.FillColor = Color.FromHex("#BF3B6EFF"); // 75%
                            }
                        }
                    }
                else if (tipologia == 1)
                {
                    foreach (var region in JsonCovid.data) // Colors based on vaccines
                    {
                        if (region.denominazione_regione == res.features[i].properties.reg_name || res.features[i].properties.reg_name == "Valle d'Aosta/Vallée d'Aoste" && region.codice_regione == 2 || res.features[i].properties.reg_name == "Trentino-Alto Adige/Südtirol" && region.codice_regione == 21 || res.features[i].properties.reg_name == "Trentino-Alto Adige/Südtirol" && region.codice_regione == 22 || res.features[i].properties.reg_name == "Friuli-Venezia Giulia" && region.denominazione_regione == "Friuli Venezia Giulia")
                        {
                            if (region.codice_regione != 21)
                            {
                                c = listaGradazioni.ElementAt(i); // Take vaine persentage of current region from the list

                                if (c < minimo + salto)
                                    polygon.FillColor = Color.FromHex("#24FF1500"); // 14%
                                else if (c < minimo + (2 * salto))
                                    polygon.FillColor = Color.FromHex("#26FF1500"); // 15%
                                else if (c < minimo + (3 * salto))
                                    polygon.FillColor = Color.FromHex("#33FF1500"); // 20%
                                else if (c < minimo + (4 * salto))
                                    polygon.FillColor = Color.FromHex("#40FF1500"); // 25%
                                else if (c < minimo + (5 * salto))
                                    polygon.FillColor = Color.FromHex("#4DFF1500"); // 30%
                                else if (c < minimo + (6 * salto))
                                    polygon.FillColor = Color.FromHex("#59FF1500"); // 35%
                                else if (c < minimo + (7 * salto))
                                    polygon.FillColor = Color.FromHex("#4DFF1500"); // 40%
                                else if (c < minimo + (8 * salto))
                                    polygon.FillColor = Color.FromHex("#4DFF1500"); // 45%
                                else if (c < minimo + (9 * salto))
                                    polygon.FillColor = Color.FromHex("#80FF1500"); // 50%
                                else if (c < minimo + (10 * salto))
                                    polygon.FillColor = Color.FromHex("#8CFF1500"); // 55%
                                else if (c < minimo + (11 * salto))
                                    polygon.FillColor = Color.FromHex("#99FF1500"); // 60%
                                else if (c < minimo + (12 * salto))
                                    polygon.FillColor = Color.FromHex("#A6FF1500"); // 65%
                                else if (c < minimo + (13 * salto))
                                    polygon.FillColor = Color.FromHex("#B3FF1500"); // 70%
                                else
                                    polygon.FillColor = Color.FromHex("#BFFF1500"); // 75%
                            }
                        }
                    }
                }
                else // Colors based on restrictions
                {
                    if (JsonRestrizioni.bianca != null)
                    {
                        if (JsonRestrizioni.bianca.Contains(res.features[i].properties.reg_name) || res.features[i].properties.reg_name == "Emilia-Romagna" && JsonRestrizioni.bianca.Contains("Emilia Romagna") || res.features[i].properties.reg_name == "Friuli-Venezia Giulia" && JsonRestrizioni.bianca.Contains("Friuli Venezia Giulia") || res.features[i].properties.reg_name == "Valle d'Aosta/Vallée d'Aoste" && JsonRestrizioni.bianca.Contains("Valle d\u2019Aosta") || res.features[i].properties.reg_name == "Trentino-Alto Adige/Südtirol" && JsonRestrizioni.bianca.Contains("Provincia autonoma di Trento"))
                            polygon.FillColor = Color.White;
                    }

                    if (JsonRestrizioni.gialla != null)
                    {
                        if (JsonRestrizioni.gialla.Contains(res.features[i].properties.reg_name) || res.features[i].properties.reg_name == "Emilia-Romagna" && JsonRestrizioni.gialla.Contains("Emilia Romagna") || res.features[i].properties.reg_name == "Friuli-Venezia Giulia" && JsonRestrizioni.gialla.Contains("Friuli Venezia Giulia") || res.features[i].properties.reg_name == "Valle d'Aosta/Vallée d'Aoste" && JsonRestrizioni.gialla.Contains("Valle d\u2019Aosta") || res.features[i].properties.reg_name == "Trentino-Alto Adige/Südtirol" && JsonRestrizioni.gialla.Contains("Provincia autonoma di Trento"))
                            polygon.FillColor = Color.Yellow;
                    }

                    if (JsonRestrizioni.arancione != null)
                    {
                        if (JsonRestrizioni.arancione.Contains(res.features[i].properties.reg_name) || res.features[i].properties.reg_name == "Emilia-Romagna" && JsonRestrizioni.arancione.Contains("Emilia Romagna") || res.features[i].properties.reg_name == "Friuli-Venezia Giulia" && JsonRestrizioni.arancione.Contains("Friuli Venezia Giulia") || res.features[i].properties.reg_name == "Valle d'Aosta/Vallée d'Aoste" && JsonRestrizioni.arancione.Contains("Valle d\u2019Aosta") || res.features[i].properties.reg_name == "Trentino-Alto Adige/Südtirol" && JsonRestrizioni.arancione.Contains("Provincia autonoma di Trento"))
                            polygon.FillColor = Color.Orange;
                    }

                    if (JsonRestrizioni.rossa != null)
                    {
                        if (JsonRestrizioni.rossa.Contains(res.features[i].properties.reg_name) || res.features[i].properties.reg_name == "Emilia-Romagna" && JsonRestrizioni.rossa.Contains("Emilia Romagna") || res.features[i].properties.reg_name == "Friuli-Venezia Giulia" && JsonRestrizioni.rossa.Contains("Friuli Venezia Giulia") || res.features[i].properties.reg_name == "Valle d'Aosta/Vallée d'Aoste" && JsonRestrizioni.rossa.Contains("Valle d\u2019Aosta") || res.features[i].properties.reg_name == "Trentino-Alto Adige/Südtirol" && JsonRestrizioni.rossa.Contains("Provincia autonoma di Trento"))
                            polygon.FillColor = Color.Red;
                    }
                }

                var arr = res.features[i].geometry.coordinates.ToArray();

                if (res.features[i].geometry.type == "Polygon") // If polygon
                {
                    foreach (var item in arr[0])
                    {
                        valore1 = double.Parse(item[1].ToString());
                        valore2 = double.Parse(item[0].ToString());
                        polygon.Geopath.Add(new Position(valore1, valore2));
                    }
                }
                else // If multipolygon
                {
                    object[] confine = arr[0][0];

                    foreach (var s in confine)
                    {
                        JToken token = (JToken)s;
                        var pnt = token.Children().ToArray();

                        polygon.Geopath.Add(new Position((double)pnt[1], (double)pnt[0]));
                    }
                }

                regionsPoly[i] = polygon;
            }

            return regionsPoly;
        }

        public async Task<List<Pin>> GetRegionsPins(int tipologia) // ANCORA DA METTERE MODALITA' COVID-19 (AL MOMENTO SOLO VACCINI)
        {
            int trenitinoVacc = 0;
            int trentinoPositivi = 0;

            List<Pin> pins = new List<Pin>();
            Pin pin = new Pin();

            if (tipologia == 0)
            {
                string dati = await client.GetStringAsync(vacciniSummaryGet);
                JsonColors = JsonConvert.DeserializeObject<RootobjectVacciniSummary>(dati);
            }
            else if (tipologia == 1)
            {
                string covidDati = "{ \n\"schema\":{\n\"fields\":[\n{\n\"name\":\"data\",\n\"type\":\"datetime\" \n}, \n{ \n\"name\":\"stato\", \n\"type\":\"string\" \n}, \n{ \n\"name\":\"codice_regione\", \n\"type\":\"integer\" \n}, \n{ \n\"name\":\"denominazione_regione\", \n\"type\":\"string\" \n}, \n{ \n\"name\":\"lat\", \n\"type\":\"float\" \n}, \n{ \n\"name\":\"long\", \n\"type\":\"float\" \n}, \n{ \n\"name\":\"ricoverati_con_sintomi\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"terapia_intensiva\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"totale_ospedalizzati\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"isolamento_domiciliare\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"totale_positivi\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"variazione_totale_positivi\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"nuovi_positivi\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"dimessi_guariti\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"deceduti\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"casi_da_sospetto_diagnostico\", \n\"type\":\"object\" \n}, \n{ \n\"name\":\"casi_da_screening\", \n\"type\":\"object\" \n}, \n{ \n\"name\":\"totale_casi\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"tamponi\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"casi_testati\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"note\", \n\"type\":\"string\" \n}, \n{ \n\"name\":\"ingressi_terapia_intensiva\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"note_test\", \n\"type\":\"object\" }, \n{ \n\"name\":\"note_casi\", \n\"type\":\"string\" \n}, \n{ \n\"name\":\"totale_positivi_test_molecolare\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"totale_positivi_test_antigenico_rapido\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"tamponi_test_molecolare\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"tamponi_test_antigenico_rapido\", \n\"type\":\"int\" \n}, \n{ \n\"name\":\"codice_nuts_1\", \n\"type\":\"string\" \n}, \n{ \n\"name\":\"codice_nuts_2\", \n\"type\":\"string\" \n}, \n], \n}, \n\"data\":";
                covidDati += await client.GetStringAsync(covidGet);
                covidDati += "}";
                JsonCovid = JsonConvert.DeserializeObject<RootobjectCovid>(covidDati);
            }

            if (tipologia == 0 || tipologia == 2) // Pins for vaccines
                foreach (var data in JsonColors.data)
                {
                    pin = new Pin();

                    if (data.index == 11)
                        trenitinoVacc += data.dosi_somministrate;
                    else if (data.index == 12)
                    {
                        trenitinoVacc += data.dosi_somministrate;

                        var locations = await Geocoding.GetLocationsAsync("Trentino Alto Adige");
                        var location = locations.FirstOrDefault();

                        pin.Label = "Trentino Alto Adige";
                        if (tipologia == 0)
                            pin.Address = $"{trenitinoVacc} somministrati.";
                        pin.Position = new Position(location.Latitude, location.Longitude);
                        pin.Type = PinType.Place;

                        pins.Add(pin);
                    }
                    else
                    {
                        var locations = await Geocoding.GetLocationsAsync(data.nome_area + " " + data.nome_area);
                        var location = locations.FirstOrDefault();

                        pin.Label = data.nome_area;
                        if (tipologia == 0)
                            pin.Address = $"{data.dosi_somministrate} somministrati.";
                        pin.Position = new Position(location.Latitude, location.Longitude);
                        pin.Type = PinType.Place;

                        pins.Add(pin);
                    }
                }
            else if (tipologia == 1) // Pins for covid
                foreach (var data in JsonCovid.data)
                {
                    pin = new Pin();

                    if (data.codice_regione == 21)
                        trentinoPositivi += data.totale_positivi;
                    else if (data.codice_regione == 22)
                    {
                        trentinoPositivi += data.totale_positivi;

                        var locations = await Geocoding.GetLocationsAsync("Trentino Alto Adige");
                        var location = locations.FirstOrDefault();

                        pin.Label = "Trentino Alto Adige";
                        pin.Address = $"{trentinoPositivi} positivi.";
                        pin.Position = new Position(location.Latitude, location.Longitude);
                        pin.Type = PinType.Place;

                        pins.Add(pin);
                    }
                    else
                    {
                        var locations = await Geocoding.GetLocationsAsync(data.denominazione_regione + " " + data.denominazione_regione);
                        var location = locations.FirstOrDefault();

                        pin.Label = data.denominazione_regione;
                        pin.Address = $"{data.totale_positivi} positivi.";
                        pin.Position = new Position(location.Latitude, location.Longitude);
                        pin.Type = PinType.Place;

                        pins.Add(pin);
                    }
                }

            return pins;
        }
    }
}