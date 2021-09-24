using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using TimesheetMobileApp2021.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Text;

namespace TimesheetMobileApp2021
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WorkAssignmentPage : ContentPage
    {

        int eId;
        string lat;
        string lon;

        HttpClientHandler GetInsecureHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
            return handler;
        }


        public WorkAssignmentPage(int id)
        {
            InitializeComponent();

            eId = id;

            //Annetaan latausilmoitukset
            lon_label.Text = "Sijaintia haetaan";
            wa_lataus.Text = "Ladataan työtehtäviä...";

            GetCurrentLocation();


            //------sijainnin haku ja näyttäminen-----------------------

            async void GetCurrentLocation()
            {
                try
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

                    var location = await Geolocation.GetLocationAsync(request);

                    if (location != null)
                    {

                        lon_label.Text = "Longitude: " + location.Longitude;
                        lat_label.Text = $"Latitude: {location.Latitude}";

                        lat = location.Latitude.ToString();
                        lon = location.Longitude.ToString();

                    }
                }
                catch (FeatureNotSupportedException fnsEx)
                {
                    await DisplayAlert("Virhe", fnsEx.ToString(), "ok");
                }
                catch (FeatureNotEnabledException fneEx)
                {
                    await DisplayAlert("Virhe", fneEx.ToString(), "ok");
                }
                catch (PermissionException pEx)
                {
                    await DisplayAlert("Virhe", pEx.ToString(), "ok");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Virhe", ex.ToString(), "ok");
                }

            }



            // -------------------------------------------------------

            LoadDataFromRestAPI();

            async void LoadDataFromRestAPI()
            {
                try
                {
                   

#if DEBUG
                    HttpClientHandler insecureHandler = GetInsecureHandler();
                    HttpClient client = new HttpClient(insecureHandler);
#else
                    HttpClient client = new HttpClient();
#endif
                    client.BaseAddress = new Uri("https://10.0.2.2:5001/");
                    string json = await client.GetStringAsync("api/workassignments");

                    IEnumerable<Workassignments> wa = JsonConvert.DeserializeObject<Workassignments[]>(json);
                    
                    ObservableCollection<Workassignments> dataa = new ObservableCollection<Workassignments>(wa);
                    dataa = dataa;

                    // Asetetaan datat näkyviin xaml tiedostossa olevalle listalle
                    waList.ItemsSource = dataa;

                    // Tyhjennetään latausilmoitus label
                    wa_lataus.Text = "";

                }

                catch (Exception e)
                {
                    await DisplayAlert("Virhe", e.Message.ToString(), "SELVÄ!");

                }
            }
        }

 

        async void startbutton_Clicked(object sender, EventArgs e)
        {
            Workassignments wa = (Workassignments)waList.SelectedItem;

            if (wa == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse työtehtävä.", "OK");
                return;
            }
            

            try
            {
                Operation op = new Operation
                {
                    EmployeeID = eId,
                    WorkAssignmentID = wa.IdWorkAssignment,
                    CustomerID = wa.IdCustomer,
                    OperationType = "start",
                    Comment = "Aloitettu",
                    Latitude = lat,
                    Longitude = lon
                };

         

#if DEBUG
                HttpClientHandler insecureHandler = GetInsecureHandler();
                HttpClient client = new HttpClient(insecureHandler);
#else
                    HttpClient client = new HttpClient();
#endif
                client.BaseAddress = new Uri("https://10.0.2.2:5001/");

                // Muutetaan em. data objekti Jsoniksi
                string input = JsonConvert.SerializeObject(op);
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                // Lähetetään serialisoitu objekti back-endiin Post pyyntönä
                HttpResponseMessage message = await client.PostAsync("/api/workassignments", content);


                // Otetaan vastaan palvelimen vastaus
                string reply = await message.Content.ReadAsStringAsync();

                //Asetetaan vastaus serialisoituna success muuttujaan
                bool success = JsonConvert.DeserializeObject<bool>(reply);

                if (success == false)
                {
                    await DisplayAlert("Ei voida aloittaa", "Työ on jo käynnissä", "OK");
                }
                else if (success == true)
                {
                    await DisplayAlert("Työ aloitettu", "Työ on aloitettu", "OK");
                }

            }

            catch (Exception ex)
            {
                await DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }


        }




        async void stopbutton_Clicked(object sender, EventArgs e)
        {

            Workassignments wa = (Workassignments)waList.SelectedItem;

            if (wa == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse työtehtävä.", "OK");
                return;
            }

            string result = await DisplayPromptAsync("Kommentti", "Kirjoita kommentti");

            try
            {

                Operation op = new Operation
                {
                    EmployeeID = eId,
                    WorkAssignmentID = wa.IdWorkAssignment,
                    CustomerID = wa.IdCustomer,
                    OperationType = "stop",
                    Comment = result,
                    Latitude = lat,
                    Longitude = lon
                };


#if DEBUG
                HttpClientHandler insecureHandler = GetInsecureHandler();
                HttpClient client = new HttpClient(insecureHandler);
#else
                    HttpClient client = new HttpClient();
#endif
                client.BaseAddress = new Uri("https://10.0.2.2:5001/");

                // Muutetaan em. data objekti Jsoniksi
                string input = JsonConvert.SerializeObject(op);
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                // Lähetetään serialisoitu objekti back-endiin Post pyyntönä
                HttpResponseMessage message = await client.PostAsync("/api/workassignments", content);


                // Otetaan vastaan palvelimen vastaus
                string reply = await message.Content.ReadAsStringAsync();

                //Asetetaan vastaus serialisoituna success muuttujaan
                bool success = JsonConvert.DeserializeObject<bool>(reply);

                if (success == false)
                {
                    await DisplayAlert("Ei voida lopettaa", "Työtä ei ole aloitettu", "OK");
                }

                else if (success == true)
                {
                    await DisplayAlert("Työn päättyminen", "Työ on päättynyt", "OK");

                    await Navigation.PushAsync(new WorkAssignmentPage(eId));
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }
        }
    }
}