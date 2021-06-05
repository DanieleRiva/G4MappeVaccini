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

        public MainPage()
        {
            InitializeComponent();

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
                Map.Pins.Clear();
        }

        private async void OnPointsToggled(object sender, ToggledEventArgs e)
        {
            if (PinSwitch.IsToggled && !pinSwitched)
            {
                LoadingRing.IsRunning = true;
                PinSwitch.IsEnabled = false;
                pinSwitched = true;

                pinSomministrazione = await MappaPins.GetPinData();

                Map.Pins.Clear();

                pinSomministrazione.ForEach(pin => Map.Pins.Add(pin));
                PinSwitch.IsEnabled = true;
            }
            else if (PinSwitch.IsToggled && pinSwitched) // Prevents from loading more than once
            {
                LoadingRing.IsRunning = true;
                Map.Pins.Clear();
                pinSomministrazione.ForEach(pin => Map.Pins.Add(pin));
            }
            else
            {
                Map.Pins.Clear();
                covidPinRegions.ForEach(pin => Map.Pins.Add(pin));
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
            }
            else if (((Button)sender).Text == "Vaccini" && vaccinesLoaded)
            {
                foreach (var poly in vaccinesPolygons)
                    Map.MapElements.Add(poly);

                VacciniButton.TextColor = Color.Red;
                RestrizioniButton.TextColor = Color.Black;
                CovidButton.TextColor = Color.Black;

                if (PinSwitch.IsToggled)
                    OnRegionsPinsToggled(null, null);
            }
            else if (((Button)sender).Text == "Covid-19" && !covidLoaded) // Clicked Covid-19
            {
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
            }
            else if (((Button)sender).Text == "Covid-19" && covidLoaded)
            {
                foreach (var poly in covidPolygons)
                    Map.MapElements.Add(poly);

                VacciniButton.TextColor = Color.Black;
                RestrizioniButton.TextColor = Color.Black;
                CovidButton.TextColor = Color.Red;

                if (PinSwitch.IsToggled)
                    OnRegionsPinsToggled(null, null);
            }
            else if (((Button)sender).Text == "Restrizioni" && !restrictionsLoaded) // Clicked restrictions
            {
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
            }
            else if (((Button)sender).Text == "Restrizioni" && restrictionsLoaded)
            {
                foreach (var poly in retrictionsPolygons)
                    Map.MapElements.Add(poly);

                VacciniButton.TextColor = Color.Black;
                RestrizioniButton.TextColor = Color.Red;
                CovidButton.TextColor = Color.Black;

                if (PinSwitch.IsToggled)
                    OnRegionsPinsToggled(null, null);
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