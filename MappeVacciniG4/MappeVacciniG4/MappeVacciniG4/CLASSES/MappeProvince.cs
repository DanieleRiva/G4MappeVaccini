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

   
    class MappeProvince
    {
        public async Task<Polygon[]> InitData()
        {
            Rootobject res = new Rootobject();

            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(EmptyClass)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("MappeVacciniG4.GEOJSON.province.geojson");

            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                res = JsonConvert.DeserializeObject<Rootobject>(json);
            }

            Polygon[] regionsPoly = new Polygon[110];

            double valore1 = 0;
            double valore2 = 0;

            Color[] colors = new Color[3];
            colors[0] = Color.FromHex("#4C5CFF8F"); // green
            colors[1] = Color.FromHex("#4CFF0E00"); // red
            colors[2] = Color.FromHex("#4CFFCF1D"); // yellow

            Random random = new Random();

            for (int i = 0; i < 110; i++)
            {
                Polygon polygon = new Polygon();
                int r = random.Next(0, 3);
                polygon.StrokeColor = Color.DarkSlateGray; // Random color generator
                polygon.FillColor = colors[r]; // Random color generator
                polygon.StrokeWidth = 5;

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

            await Geocoding.GetLocationsAsync("Milano");

            return regionsPoly;
        }
    }
}
