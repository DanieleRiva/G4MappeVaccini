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
    public partial class MainPage : ContentPage
    {
        MappeRegioni MappeRegioni = new MappeRegioni();
        List<Pin> regionsPin = new List<Pin>();
        MappeProvince MappeProvince = new MappeProvince();
        MapPins MappaPins = new MapPins();

        Polygon[] regionsPolygons;
        Polygon[] retrictionsPolygons;
        //Polygon[] provincePolygons;
        List<Pin> pinSomministrazione = new List<Pin>();
        bool pinSwitched = false;
        //bool provinceLoaded = false;
        bool regionsLoaded = false;

        public MainPage()
        {
            InitializeComponent();

            //Distance prova = new Distance(34.345);
            //Map.MoveToRegion(prova);
            //Position position = new Position(36.9628066, -122.0194722);
            //MapSpan mapSpan = new MapSpan(position, 0.01, 0.01);
            //Map = mapSpan;

            OnRegions(null , null); // Load regions when launching app
        }

        private async void OnPinsToggled(object sender, ToggledEventArgs e)
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
                regionsPin.ForEach(pin => Map.Pins.Add(pin));
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

            if (RegionsButton.TextColor == Color.Black && !regionsLoaded)
            {
                regionsPolygons = await MappeRegioni.InitData(false);
                regionsPin = await MappeRegioni.GetRegionsPins(false);

                foreach (var poly in regionsPolygons)
                    Map.MapElements.Add(poly);

                regionsPin.ForEach(pin => Map.Pins.Add(pin));

                RegionsButton.TextColor = Color.Red;
                RestrizioniButton.TextColor = Color.Black;
                //ProvinceButton.TextColor = Color.Black;
            }
            else if (RegionsButton.TextColor == Color.Black && regionsLoaded)
            {
                foreach (var poly in regionsPolygons)
                    Map.MapElements.Add(poly);

                regionsPin.ForEach(pin => Map.Pins.Add(pin));

                RegionsButton.TextColor = Color.Red;
                RestrizioniButton.TextColor = Color.Black;
                //ProvinceButton.TextColor = Color.Black;
            }
            else if (((Button)sender).Text == "Restrizioni") // Clicked restrictions
            {
                retrictionsPolygons = await MappeRegioni.InitData(true);
                regionsPin = await MappeRegioni.GetRegionsPins(true);

                foreach (var poly in retrictionsPolygons)
                    Map.MapElements.Add(poly);

                regionsPin.ForEach(pin => Map.Pins.Add(pin));

                RegionsButton.TextColor = Color.Black;
                RestrizioniButton.TextColor = Color.Red;
            }
        }

        private void OnRestrictions(object sender, EventArgs e)
        {
            //MappeRegioni.InitData(true);
        }

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