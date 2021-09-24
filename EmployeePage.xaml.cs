using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using TimesheetMobileApp2021.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace TimesheetMobileApp2021
{
    public partial class EmployeePage : ContentPage
    {
        // Muuttujan alustaminen
        ObservableCollection<Employee> dataa = new ObservableCollection<Employee>();

        
        public EmployeePage()
        {
            InitializeComponent();

            LoadDataFromRestAPI();


            //Annetaan latausilmoitus
            emp_lataus.Text = "Ladataan työntekijöitä...";
   
            async void LoadDataFromRestAPI()
            {
                try
                {
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

#if DEBUG
                    HttpClientHandler insecureHandler = GetInsecureHandler();
                    HttpClient client = new HttpClient(insecureHandler);
#else
                    HttpClient client = new HttpClient();
#endif
                     client.BaseAddress = new Uri("https://10.0.2.2:5001/");
                    string json = await client.GetStringAsync("api/employees");

                    IEnumerable<Employee> employees = JsonConvert.DeserializeObject<Employee[]>(json);
                    // dataa -niminen observableCollection on alustettukin jo ylhäällä päätasolla että hakutoiminto,
                    // pääsee siihen käsiksi.
                    // asetetaan sen sisältö ensi kerran tässä pienellä kepulikonstilla:
                    ObservableCollection<Employee> dataa2 = new ObservableCollection<Employee>(employees);
                    dataa = dataa2;

                    // Asetetaan datat näkyviin xaml tiedostossa olevalle listalle
                    employeeList.ItemsSource = dataa;

                    // Tyhjennetään latausilmoitus label
                    emp_lataus.Text = "";

                }

                catch (Exception e)
                {
                    await DisplayAlert("Virhe", e.Message.ToString(), "SELVÄ!");
                
                }
            }
        }

        // Hakutoiminto
        private void OnSearchBarButtonPressed(object sender, EventArgs args)
        {
            SearchBar searchBar = (SearchBar)sender;
            string searchText = searchBar.Text;

            // Työntekijälistaukseen valitaan nyt vain ne joiden etu- tai sukunimeen sisältyy annettu hakutermi
            // "var dataa" on tiedoston päätasolla alustettu muuttuja, johon sijoitettiin alussa koko lista työntekijöistä.
            // Nyt siihen sijoitetaan vain hakuehdon täyttävät työntekijät
            employeeList.ItemsSource = dataa.Where(x => x.LastName.ToLower().Contains(searchText.ToLower())
            || x.FirstName.ToLower().Contains(searchText.ToLower()));

        }

       // Sivun päivitys jos palataan hakutilanteesta koko listaukseen
        async void päivitysButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EmployeePage());
        }



        async void navibutton_Clicked(object sender, EventArgs e)
        {
            Employee emp = (Employee)employeeList.SelectedItem;

            if (emp == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse työntekijä.", "OK"); // (otsikko, teksti, kuittausnapin teksti)
                return;
            }
            else
            {

                int id = emp.IdEmployee;
                await Navigation.PushAsync(new WorkAssignmentPage(id)); // Navigoidaan uudelle sivulle
            }
        }
    }

}