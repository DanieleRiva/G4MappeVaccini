using MappeVacciniG4.CLASSES;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MappeVacciniG4
{
    // 0 = vaccini
    // 1 = covid
    // 2 = restrizioni
    public partial class MainPage : ContentPage
    {
        string[] regions = new string[] { "Piemonte", "Valle d'Aosta / Vallée d'Aoste", "Lombardia", "Trentino Alto Adige", "Veneto", "Friuli-Venezia Giulia", "Liguria", "Emilia-Romagna", "Toscana", "Umbria", "Marche", "Lazio", "Abruzzo", "Molise", "Campania", "Puglia", "Basilicata", "Calabria", "Sicilia", "Sardegna" };

        MappeRegioni MappeRegioni = new MappeRegioni();
        List<Pin> vaccinesPinsRegions = new List<Pin>();
        List<Pin> covidPinRegions = new List<Pin>();
        List<Pin> restrictionsPinRegions = new List<Pin>();
        //MappeProvince MappeProvince = new MappeProvince();
        MapPins MappaPins = new MapPins();

        Polygon[] vaccinesPolygons;
        Polygon[] covidPolygons;
        Polygon[] retrictionsPolygons;
        //Polygon[] provincePolygons;
        List<Pin> pinSomministrazione = new List<Pin>();
        bool pinSwitched = false;
        //bool provinceLoaded = false;
        bool vaccinesLoaded = false;
        bool covidLoaded = false;
        bool restrictionsLoaded = false;
        bool changedByError = false;

        public Location location;

        public MainPage()
        {
            InitializeComponent();

            OnStartupGps();

            Position initPos = new Position(41.90261250766303, 12.496868574122308); // Set initial zoom level
            Map.MoveToRegion(new MapSpan(initPos, 13, 13));

            OnRegions(null, null); // Load regions when launching app
        }

        private void OnRegionsPinsToggled(object sender, ToggledEventArgs e)
        {
            if (PinSwitch.IsToggled && VacciniButton.TextColor == Color.Red)
                vaccinesPinsRegions.ForEach(pin => Map.Pins.Add(pin));
            else if (PinSwitch.IsToggled && CovidButton.TextColor == Color.Red)
                covidPinRegions.ForEach(pin => Map.Pins.Add(pin));
            else if (PinSwitch.IsToggled && RestrizioniButton.TextColor == Color.Red)
                restrictionsPinRegions.ForEach(pin => Map.Pins.Add(pin));
            else
                foreach (var pin in Map.Pins.ToList())
                    if (regions.Contains(pin.Label) || pin.Label == "Friuli Venezia Giulia" || pin.Label == "Valle d'Aosta")
                        Map.Pins.Remove(pin);
        }

        private async void OnPointsToggled(object sender, ToggledEventArgs e)
        {
            changedByError = false;

            if (CentriSwitch.IsToggled && !pinSwitched)
            {
                LoadingRing.IsRunning = true;
                CentriSwitch.IsEnabled = false;

                pinSomministrazione = await MappaPins.GetPinData();

                if (pinSomministrazione == null)
                {
                    await DisplayAlert("Attenzione", "Non è stato possibile rilevare la tua posizione.", "Ok");
                    CentriSwitch.IsToggled = false;
                    changedByError = true;
                }
                else
                {
                    pinSomministrazione.ForEach(pin => Map.Pins.Add(pin));
                    pinSwitched = true;
                }

                CentriSwitch.IsEnabled = true;
            }
            else if (CentriSwitch.IsToggled && pinSwitched) // Prevents from loading more than once
            {
                LoadingRing.IsRunning = true;

                foreach (var pin in Map.Pins.ToList())
                    if (!regions.Contains(pin.Label))
                        Map.Pins.Remove(pin);

                pinSomministrazione.ForEach(pin => Map.Pins.Add(pin));
            }
            else if (!changedByError)
            {
                foreach (var pin in Map.Pins.ToList())
                    if (!regions.Contains(pin.Label))
                        Map.Pins.Remove(pin);
            }

            LoadingRing.IsRunning = false;
        }

        private void OnSatelliteToggle(object sender, ToggledEventArgs e)
        {
            if (Map.MapType == MapType.Street)
                Map.MapType = MapType.Hybrid;
            else
                Map.MapType = MapType.Street;
        }

        private void OnOpenInfoPage(object sender, EventArgs e)
        {
            InfoStackLayout.IsVisible = true;
        }

        private void OnCloseInfoPage(object sender, EventArgs e)
        {
            InfoStackLayout.IsVisible = false;
        }

        private async void OnRegions(object sender, EventArgs e)
        {
            Map.MapElements.Clear();
            Map.Pins.Clear();

            if (sender == null || ((Button)sender).Text == "Vaccini" && !vaccinesLoaded)
            {
                OnOpenToolbar(null, null);

                vaccinesPolygons = await MappeRegioni.InitData(0);
                vaccinesPinsRegions = await MappeRegioni.GetRegionsPins(0);

                foreach (var poly in vaccinesPolygons)
                    Map.MapElements.Add(poly);

                VacciniButton.TextColor = Color.Red;
                RestrizioniButton.TextColor = Color.Black;
                CovidButton.TextColor = Color.Black;

                if (PinSwitch.IsToggled)
                    OnRegionsPinsToggled(null, null);

                vaccinesLoaded = true;

                //CentersStakPanel.IsVisible = true;
            }
            else if (((Button)sender).Text == "Vaccini" && vaccinesLoaded)
            {
                OnOpenToolbar(null, null);

                foreach (var poly in vaccinesPolygons)
                    Map.MapElements.Add(poly);

                VacciniButton.TextColor = Color.Red;
                RestrizioniButton.TextColor = Color.Black;
                CovidButton.TextColor = Color.Black;

                if (PinSwitch.IsToggled)
                    OnRegionsPinsToggled(null, null);

                //CentersStakPanel.IsVisible = true;
            }
            else if (((Button)sender).Text == "Covid-19" && !covidLoaded) // Clicked Covid-19
            {
                OnOpenToolbar(null, null);

                covidPolygons = await MappeRegioni.InitData(1);
                covidPinRegions = await MappeRegioni.GetRegionsPins(1);

                foreach (var poly in covidPolygons)
                    Map.MapElements.Add(poly);

                VacciniButton.TextColor = Color.Black;
                RestrizioniButton.TextColor = Color.Black;
                CovidButton.TextColor = Color.Red;

                if (PinSwitch.IsToggled)
                    OnRegionsPinsToggled(null, null);

                covidLoaded = true;

                //CentersStakPanel.IsVisible = false;
            }
            else if (((Button)sender).Text == "Covid-19" && covidLoaded)
            {
                OnOpenToolbar(null, null);

                foreach (var poly in covidPolygons)
                    Map.MapElements.Add(poly);

                VacciniButton.TextColor = Color.Black;
                RestrizioniButton.TextColor = Color.Black;
                CovidButton.TextColor = Color.Red;

                if (PinSwitch.IsToggled)
                    OnRegionsPinsToggled(null, null);

                //CentersStakPanel.IsVisible = false;
            }
            else if (((Button)sender).Text == "Restrizioni" && !restrictionsLoaded) // Clicked restrictions
            {
                OnOpenToolbar(null, null);

                retrictionsPolygons = await MappeRegioni.InitData(2);
                restrictionsPinRegions = await MappeRegioni.GetRegionsPins(2);

                foreach (var poly in retrictionsPolygons)
                    Map.MapElements.Add(poly);

                VacciniButton.TextColor = Color.Black;
                RestrizioniButton.TextColor = Color.Red;
                CovidButton.TextColor = Color.Black;

                if (PinSwitch.IsToggled)
                    OnRegionsPinsToggled(null, null);

                restrictionsLoaded = true;
                //CentersStakPanel.IsVisible = false;
            }
            else if (((Button)sender).Text == "Restrizioni" && restrictionsLoaded)
            {
                OnOpenToolbar(null, null);

                foreach (var poly in retrictionsPolygons)
                    Map.MapElements.Add(poly);

                VacciniButton.TextColor = Color.Black;
                RestrizioniButton.TextColor = Color.Red;
                CovidButton.TextColor = Color.Black;

                if (PinSwitch.IsToggled)
                    OnRegionsPinsToggled(null, null);

                //CentersStakPanel.IsVisible = false;
            }
        }

        private async void OnStartupGps()
        {
            try
            {
                location = await Geolocation.GetLastKnownLocationAsync();

                if (location == null)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                    location = await Geolocation.GetLocationAsync(request);

                    if (location == null)
                        await DisplayAlert("Attenzione", "Non è stato possibile rilevare la tua posizione.", "Ok");
                }
            }
            catch (Exception)
            {
                await DisplayAlert("Attenzione", "Non è stato possibile rilevare la tua posizione.", "Ok");
            }
        }

        private void OnOpenToolbar(object sender, EventArgs e)
        {
            if (sender != null && buttonToolbar.Source.ToString() == "File: downArrow.png") // open toolbar
            {
                buttonToolbar.Source = "upArrow.png";

                PinStackLayout.IsVisible = true;

                if (VacciniButton.TextColor == Color.Red)
                    CentersStakPanel.IsVisible = true;

                SatelliteStackLayout.IsVisible = true;
            }
            else if (sender == null) // close toolbar from other methods
            {
                buttonToolbar.Source = "downArrow.png";

                PinStackLayout.IsVisible = false;
                CentersStakPanel.IsVisible = false;
                SatelliteStackLayout.IsVisible = false;
            }
            else // close toolbar
            {
                buttonToolbar.Source = "downArrow.png";

                PinStackLayout.IsVisible = false;
                CentersStakPanel.IsVisible = false;
                SatelliteStackLayout.IsVisible = false;
            }
        }

        //private void OnRestrictions(object sender, EventArgs e)
        //{
        //    //MappeRegioni.InitData(true);
        //}

        //private async void OnCovid(object sender, EventArgs e)
        //{
        //    Map.MapElements.Clear();
        //    Map.Pins.Clear();

        //    if (CovidButton.TextColor == Color.Black && !regionsLoaded)
        //    {
        //        regionsPolygons = await MappeRegioni.InitData(1);
        //        regionsPin = await MappeRegioni.GetRegionsPins(false);

        //        foreach (var poly in regionsPolygons)
        //            Map.MapElements.Add(poly);

        //        regionsPin.ForEach(pin => Map.Pins.Add(pin));

        //        RegionsButton.TextColor = Color.Black;
        //        RestrizioniButton.TextColor = Color.Black;
        //        //ProvinceButton.TextColor = Color.Black;
        //        CovidButton.TextColor = Color.Red;
        //    }
        //}

        //private async void OnProvince(object sender, EventArgs e)
        //{
        //    if (ProvinceButton.TextColor == Color.Black && !provinceLoaded)
        //    {
        //        //LoadingRing.IsRunning = true;

        //        Map.MapElements.Clear();
        //        provincePolygons = await MappeProvince.InitData();
        //        foreach (var poly in provincePolygons)
        //            Map.MapElements.Add(poly);

        //        ProvinceButton.TextColor = Color.Red;
        //        RegionsButton.TextColor = Color.Black;

        //        //provinceLoaded = true;
        //    }
        //    else if (ProvinceButton.TextColor == Color.Black && provinceLoaded)
        //    {
        //        //LoadingRing.IsRunning = true;
        //        Map.MapElements.Clear();

        //        foreach (var poly in provincePolygons)
        //            Map.MapElements.Add(poly);

        //        ProvinceButton.TextColor = Color.Red;
        //        RegionsButton.TextColor = Color.Black;
        //    }

        //    LoadingRing.IsRunning = false;
        //}
    }
}