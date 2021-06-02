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
        static readonly HttpClient client = new HttpClient();
        static string vacciniSummaryGet = "https://raw.githubusercontent.com/italia/covid19-opendata-vaccini/master/dati/vaccini-summary-latest.json";
        static string restrizioniGet = "https://covid19.zappi.me/coloreRegioni.php";
        List<float> listaGradazioni = new List<float>();
        public float[] popolazione = { 4356000, 125666, 10060000, 1072000, 4906000, 1215000, 1551000, 4459000, 3730000, 882015, 1525000, 5879000, 1312000, 305617, 5802000, 4029000, 562869, 1947000, 5000000, 1640000 };

        RootobjectVacciniSummary JsonColors = new RootobjectVacciniSummary();
        float c = 0;

        string restrizioniDati = string.Empty;
        Restrizioni JsonRestrizioni = new Restrizioni();

        public async Task<Polygon[]> InitData(bool restrizioni)
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

            if (!restrizioni)
            {
                string dati = await client.GetStringAsync(vacciniSummaryGet);
                JsonColors = JsonConvert.DeserializeObject<RootobjectVacciniSummary>(dati);
            }
            else
            {
                restrizioniDati = await client.GetStringAsync(restrizioniGet);
                JsonRestrizioni = JsonConvert.DeserializeObject<Restrizioni>(restrizioniDati);
            }

            if (!restrizioni) // Find Maximum and minimum persentage of vaccines
            {
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

            for (int i = 0; i < 20; i++) // Create polygons scrolling throught regions
            {
                Polygon polygon = new Polygon();
                polygon.StrokeWidth = 5;
                polygon.StrokeColor = Color.DarkSlateGray;

                if (!restrizioni)
                    foreach (var region in JsonColors.data) // Colors based on vaccines
                    {
                        if (region.nome_area == res.features[i].properties.reg_name || res.features[i].properties.reg_name == "Valle d'Aosta/Vallée d'Aoste" && region.index == 19 || res.features[i].properties.reg_name == "Trentino-Alto Adige/Südtirol" && region.index == 11 || res.features[i].properties.reg_name == "Trentino-Alto Adige/Südtirol" && region.index == 12)
                        {
                            if (region.index != 11)
                            {
                                c = listaGradazioni.ElementAt(i); // Take vaine persentage of current region from the list

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

        public async Task<List<Pin>> GetRegionsPins(bool restrizioni)
        {
            int trenitinoVacc = 0;

            List<Pin> pins = new List<Pin>();
            Pin pin = new Pin();

            string dati = await client.GetStringAsync(vacciniSummaryGet);
            RootobjectVacciniSummary Json = JsonConvert.DeserializeObject<RootobjectVacciniSummary>(dati);

            foreach (var data in Json.data)
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
                    if (!restrizioni)
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
                    if (!restrizioni)
                        pin.Address = $"{data.dosi_somministrate} somministrati.";
                    pin.Position = new Position(location.Latitude, location.Longitude);
                    pin.Type = PinType.Place;

                    pins.Add(pin);
                }
            }

            return pins;
        }
    }
}