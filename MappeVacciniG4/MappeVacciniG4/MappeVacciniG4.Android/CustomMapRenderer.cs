using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MappeVacciniG4.CLASSES;
using MappeVacciniG4.Droid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;

[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace MappeVacciniG4.Droid
{
    public class CustomMapRenderer : MapRenderer/*, GoogleMap.IInfoWindowAdapter*/
    {
        //List<CustomPin> customPins;
        string[] regions = new string[] { "Piemonte", "Valle d'Aosta / Vallée d'Aoste", "Valle d'Aosta", "Lombardia", "Trentino Alto Adige", "Veneto", "Friuli-Venezia Giulia", "Friuli Venezia Giulia", "Liguria", "Emilia-Romagna", "Toscana", "Umbria", "Marche", "Lazio", "Abruzzo", "Molise", "Campania", "Puglia", "Basilicata", "Calabria", "Sicilia", "Sardegna" };

        public CustomMapRenderer(Context context) : base(context)
        {
        }

        //public Android.Views.View GetInfoContents(Marker marker)
        //{
        //    throw new NotImplementedException();
        //}

        //public Android.Views.View GetInfoWindow(Marker marker)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Map> e)
        //{
        //    base.OnElementChanged(e);

        //    if (e.OldElement != null)
        //    {
        //        NativeMap.InfoWindowClick -= OnInfoWindowClick;
        //    }

        //    if (e.NewElement != null)
        //    {
        //        var formsMap = (CustomMap)e.NewElement;
        //        customPins = formsMap.CustomPins;
        //    }
        //}

        //protected override void OnMapReady(GoogleMap map)
        //{
        //    base.OnMapReady(map);

        //    NativeMap.InfoWindowClick += OnInfoWindowClick;
        //    NativeMap.SetInfoWindowAdapter(this);
        //}

        protected override MarkerOptions CreateMarker(Pin pin)
        {
            var marker = new MarkerOptions();
            marker.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
            marker.SetTitle(pin.Label);
            marker.SetSnippet(pin.Address);

            if (!regions.Contains(pin.Label))
                marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.centriIcon));

            return marker;
        }
    }
}