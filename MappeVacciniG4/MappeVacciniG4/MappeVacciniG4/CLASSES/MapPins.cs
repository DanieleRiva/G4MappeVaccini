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
        string[] regions = new string[] { "Piemonte", "Valle d'Aosta / Vallée d'Aoste", "Lombardia", "Trentino Alto Adige", "Veneto", "Friuli-Venezia Giulia", "Liguria", "Emilia-Romagna", "Toscana", "Umbria", "Marche", "Lazio", "Abruzzo", "Molise", "Campania", "Puglia", "Basilicata", "Calabria", "Sicilia", "Sardegna" };

        static readonly HttpClient client = new HttpClient();
        static string urlGet = "https://raw.githubusercontent.com/italia/covid19-opendata-vaccini/master/dati/punti-somministrazione-latest.json";

        public async Task<List<Pin>> GetPinData()
        {
            Pin pin = new Pin();
            List<Pin> pins = new List<Pin>();

            string dati = await client.GetStringAsync(urlGet);
            PinsJson Json = JsonConvert.DeserializeObject<PinsJson>(dati);

            Location userLocation = await GetCurrentLocation();

            Geocoder geoCoder = new Geocoder();
            Position userPos = new Position(userLocation.Latitude, userLocation.Longitude);
            IEnumerable<string> possibleAddresses = await geoCoder.GetAddressesForPositionAsync(userPos);
            string currentPos = possibleAddresses.FirstOrDefault();

            string capString = currentPos.Split(',')[2];

            capString = capString.Split(' ')[1];
            int cap = int.Parse(capString);

            if (cap >= 64010 && cap <= 67100)
                currentPos = "Abruzzo";
            else if (cap >= 75010 && cap <= 85100)
                currentPos = "Basilicata";
            else if (cap >= 87010 && cap <= 89900)
                currentPos = "Calabria";
            else if (cap >= 80010 && cap <= 84135)
                currentPos = "Campania";
            else if (cap >= 29010 && cap <= 48100)
                currentPos = "Emilia-Romagna";
            else if (cap >= 33010 && cap <= 34170)
                currentPos = "Friuli-Venezia Giulia";
            else if (cap >= 00010 && cap <= 2011)
                currentPos = "Lazio";
            else if (cap >= 12071 && cap <= 19137)
                currentPos = "Liguria";
            else if (cap >= 16192 && cap <= 46100)
                currentPos = "Lombardia";
            else if (cap >= 60010 && cap <= 63900)
                currentPos = "Marche";
            else if (cap >= 86010 && cap <= 86170)
                currentPos = "Molise";
            else if (cap >= 10010 && cap <= 28925)
                currentPos = "Piemonte";
            else if (cap >= 70010 && cap <= 76125)
                currentPos = "Puglia";
            else if (cap >= 07010 && cap <= 09170)
                currentPos = "Sardegna";
            else if (cap >= 90010 && cap <= 98168)
                currentPos = "Sicilia";
            else if (cap >= 50010 && cap <= 59100)
                currentPos = "Toscana";
            else if (cap >= 38010 && cap <= 39100)
                currentPos = "Provincia Autonoma Trento";
            else if (cap >= 05010 && cap <= 05010)
                currentPos = "Umbria";
            else if (cap >= 11010 && cap <= 11010)
                currentPos = @"Valle d'Aosta \/ Vall\u00e9e d'Aoste";
            else if (cap >= 30010 && cap <= 45100)
                currentPos = "Veneto";

            if (userPos != null)
            {
                foreach (var data in Json.data.Where(d => d.nome_area == currentPos))
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
            else
            {
                return null;
            }
        }

        private async Task<Location> GetCurrentLocation()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location == null)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                    //cts = new CancellationTokenSource();
                    location = await Geolocation.GetLocationAsync(request);

                    if (location != null)
                    {
                        Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                        return location;
                    }

                    return null;
                }
                else
                    return location;
            }
            catch (Exception)
            {
                // Unable to get location

                return null;
            }

        }

        //public System.Threading.Tasks.Task DisplayAlert(string title, string message, string cancel);

    }
}