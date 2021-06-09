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
        #region // VARIABLES //

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
        bool gettingPositionPins = false;

        public Location location;

        #endregion


        #region // MAINPAGE //

        public MainPage()
        {
            InitializeComponent();

            if (CheckConnection())
            {
                OnStartupGps();

                Position initPos = new Position(41.90261250766303, 12.496868574122308); // Set initial zoom level
                Map.MoveToRegion(new MapSpan(initPos, 13, 13));

                OnRegions(null, null); // Load regions when launching app
            }
            else
                DisplayAlert("Attenzione", "Non sei connesso ad internet. Connettiti e riprova.", "Ok");
        }

        #endregion


        #region // MODES // 

        private async void OnRegions(object sender, EventArgs e)
        {
            if (!gettingPositionPins && CheckConnection())
            {
                LoadingRing.IsRunning = true;

                if (sender == null || ((Button)sender).Text == "Vaccini" && VacciniButton.TextColor == Color.Black && !vaccinesLoaded)
                {
                    Map.MapElements.Clear();
                    Map.Pins.Clear();

                    OnOpenToolbar(null, null);

                    vaccinesPolygons = await MappeRegioni.InitData(0);
                    vaccinesPinsRegions = await MappeRegioni.GetRegionsPins(0);

                    await LoadVaccines();

                    VacciniButton.TextColor = Color.Red;
                    RestrizioniButton.TextColor = Color.Black;
                    CovidButton.TextColor = Color.Black;

                    if (PinSwitch.IsToggled)
                        OnRegionsPinsToggled(null, null);

                    vaccinesLoaded = true;
                }
                else if (((Button)sender).Text == "Vaccini" && VacciniButton.TextColor == Color.Black && vaccinesLoaded)
                {
                    Map.MapElements.Clear();
                    Map.Pins.Clear();

                    OnOpenToolbar(null, null);

                    await LoadVaccines();

                    VacciniButton.TextColor = Color.Red;
                    RestrizioniButton.TextColor = Color.Black;
                    CovidButton.TextColor = Color.Black;

                    if (PinSwitch.IsToggled)
                        OnRegionsPinsToggled(null, null);
                }
                else if (((Button)sender).Text == "Covid-19" && CovidButton.TextColor == Color.Black && !covidLoaded) // Clicked Covid-19
                {
                    Map.MapElements.Clear();
                    Map.Pins.Clear();

                    CentriSwitch.IsToggled = false;

                    OnOpenToolbar(null, null);

                    covidPolygons = await MappeRegioni.InitData(1);
                    covidPinRegions = await MappeRegioni.GetRegionsPins(1);

                    await LoadCovid();

                    VacciniButton.TextColor = Color.Black;
                    RestrizioniButton.TextColor = Color.Black;
                    CovidButton.TextColor = Color.Red;

                    if (PinSwitch.IsToggled)
                        OnRegionsPinsToggled(null, null);

                    covidLoaded = true;
                }
                else if (((Button)sender).Text == "Covid-19" && CovidButton.TextColor == Color.Black && covidLoaded)
                {
                    Map.MapElements.Clear();
                    Map.Pins.Clear();

                    CentriSwitch.IsToggled = false;

                    OnOpenToolbar(null, null);

                    await LoadCovid();

                    VacciniButton.TextColor = Color.Black;
                    RestrizioniButton.TextColor = Color.Black;
                    CovidButton.TextColor = Color.Red;

                    if (PinSwitch.IsToggled)
                        OnRegionsPinsToggled(null, null);
                }
                else if (((Button)sender).Text == "Restrizioni" && RestrizioniButton.TextColor == Color.Black && !restrictionsLoaded) // Clicked restrictions
                {
                    Map.MapElements.Clear();
                    Map.Pins.Clear();

                    CentriSwitch.IsToggled = false;

                    OnOpenToolbar(null, null);

                    retrictionsPolygons = await MappeRegioni.InitData(2);
                    restrictionsPinRegions = await MappeRegioni.GetRegionsPins(2);

                    await LoadRestrictions();

                    VacciniButton.TextColor = Color.Black;
                    RestrizioniButton.TextColor = Color.Red;
                    CovidButton.TextColor = Color.Black;

                    if (PinSwitch.IsToggled)
                        OnRegionsPinsToggled(null, null);

                    restrictionsLoaded = true;
                }
                else if (((Button)sender).Text == "Restrizioni" && RestrizioniButton.TextColor == Color.Black && restrictionsLoaded)
                {
                    Map.MapElements.Clear();
                    Map.Pins.Clear();

                    CentriSwitch.IsToggled = false;

                    OnOpenToolbar(null, null);

                    await LoadRestrictions();

                    VacciniButton.TextColor = Color.Black;
                    RestrizioniButton.TextColor = Color.Red;
                    CovidButton.TextColor = Color.Black;

                    if (PinSwitch.IsToggled)
                        OnRegionsPinsToggled(null, null);

                }

                LoadingRing.IsRunning = false;
            }
            else if (!CheckConnection())
                await DisplayAlert("Attenzione", "Non sei connesso ad internet. Connettiti e riprova.", "Ok");
            else
                await DisplayAlert("Attenzione", "Per favore, attendi la fine del caricamento dei centri vaccinali nella tua regione.", "Ok");
        }

        #endregion

        #region // TOOLBAR //

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
            if (CheckConnection())
            {
                changedByError = false;

                gettingPositionPins = true;

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

                    await LoadPins(true);

                    pinSomministrazione.ForEach(pin => Map.Pins.Add(pin));
                }
                else if (!changedByError)
                {
                    await LoadPins(true);
                }

                gettingPositionPins = false;

                LoadingRing.IsRunning = false;
            }
            else
                await DisplayAlert("Attenzione", "Non sei connesso ad internet. Connettiti e riprova.", "Ok");
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

        #endregion

        #region // MISCELLANEOUS //

        // 0 = vaccini
        // 1 = covid
        // 2 = restrizioni

        private bool CheckConnection()
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
                return true;
            else
                return false;
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

        // load - remove //

        private async Task<bool> LoadVaccines()
        {
            await Task.Run(() =>
            {
                foreach (var poly in vaccinesPolygons)
                    Map.MapElements.Add(poly);
            });

            return true;
        }
        private async Task<bool> LoadCovid()
        {
            await Task.Run(() =>
            {
                foreach (var poly in covidPolygons)
                    Map.MapElements.Add(poly);
            });

            return true;
        }
        private async Task<bool> LoadRestrictions()
        {
            await Task.Run(() =>
            {
                foreach (var poly in retrictionsPolygons)
                    Map.MapElements.Add(poly);
            });

            return true;
        }


        private async Task<bool> LoadPins(bool load)
        {
            if (load) // Load pins
                await Task.Run(() =>
                {
                    foreach (var pin in Map.Pins.ToList())
                        if (!regions.Contains(pin.Label))
                            Map.Pins.Remove(pin);
                });

            if (!load) // Remove pins
                await Task.Run(() =>
                {
                    foreach (var pin in Map.Pins.ToList())
                        if (!regions.Contains(pin.Label))
                            Map.Pins.Remove(pin);
                });

            return true;
        }

        #endregion
    }
}