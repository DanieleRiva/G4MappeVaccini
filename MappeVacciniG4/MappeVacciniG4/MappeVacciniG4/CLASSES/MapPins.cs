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
    public class MapPins
    {
        static readonly HttpClient client = new HttpClient();
        static string urlGet = "https://raw.githubusercontent.com/italia/covid19-opendata-vaccini/master/dati/punti-somministrazione-latest.json";

        public async Task<List<Pin>> GetPinData()
        {
            Pin pin = new Pin();
            List<Pin> pins = new List<Pin>();
            string dati = await client.GetStringAsync(urlGet);

            PinsJson Json = JsonConvert.DeserializeObject<PinsJson>(dati);

            foreach (var data in Json.data)
            {
                pin = new Pin();

                var address = data.presidio_ospedaliero;
                var locations = await Geocoding.GetLocationsAsync(address + " " + data.nome_area);
                var location = locations.FirstOrDefault();

                if (location != null)
                {
                    pin.Label = data.presidio_ospedaliero;
                    //pin.Address = $"Comune: {Json.data[i].comune}  Provincia: {Json.data[i].provincia}";
                    pin.Address = $"Comune di {data.comune}";
                    pin.Position = new Position(location.Latitude, location.Longitude);
                    pin.Type = PinType.Place;

                    pins.Add(pin);
                    Debug.WriteLine(data.index + " " + pin.Label);
                }
                else
                    Debug.WriteLine(data.index + " error");
            }

            return pins;
        }

    }
}