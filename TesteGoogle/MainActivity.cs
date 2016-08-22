using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Java.Security;
using Android.Gms.Common.Apis;
using Android.Gms.Common;
using Android.Gms.Plus;
using static Android.Content.IntentSender;

namespace TesteGoogle
{
    [Activity(Label = "TesteGoogle", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {   
        // Google+ Components
        private GoogleApiClient googleApiClient;
        private ConnectionResult connectionResult;
        // Flags
        private bool isConsentScreenOpened; // Tela de consentimento aberta
        private bool isSignInClicked; // Botão de login clicado
        private int SIGN_IN_CODE = 98759;
        // Views
        private SignInButton signInButton;
        private Button buttonSignOut;
        private TextView textViewName;
        private TextView textViewEmail;
        private TextView textViewStatus;
        private TextView textViewGender;
        private TextView textViewLanguage;
        private TextView textViewNickName;
        private TextView textViewTagline;
        private TextView textViewAboutMe;
        private TextView textViewBirthday;
        private TextView textViewCurrentLocation;
        private TextView textViewId;
        private TextView textViewProfileUrl;
        private LinearLayout linearInformations;

        protected override void OnCreate(Bundle bundle)
        {
            // Removendo a Title Bar
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            
            // Vinculando Views 
            bindViewsFromXML();
            // Contruindo o objeto de requisição
            googleApiClient = new GoogleApiClient.Builder(this)
                .AddConnectionCallbacks(this)
                .AddOnConnectionFailedListener(this)
                .AddApi(PlusClass.API)
                .AddScope(new Scope(Scopes.Profile))
                .AddScope(new Scope(Scopes.PlusLogin))
                .Build();
        }

        // Evento de click do botão de login
        private void SignInButton_Click(object sender, EventArgs e)
        {
            // Verificando se estiver fazendo uma conexão à API
            if (!googleApiClient.IsConnecting)
            {
                isSignInClicked = true; // Botão de login clicado
                ResolveSignIn();
            }
        }

        // Evento de click do botão de logout
        private void ButtonSignOut_Click(object sender, EventArgs e)
        {
            // O logout só será feito caso o usuário estiver conectado à aplicação
            if (googleApiClient.IsConnected)
            {
                // Desconecta o usuário da aplicação. O acesso não está sendo revogado
                // quando o usuário for entrar na aplicação, ele terá que fazer login novamente
                PlusClass.AccountApi.ClearDefaultAccount(googleApiClient);
                // Desconecta a Google+ API
                googleApiClient.Disconnect();
            
                updateUi(false);
            }
        }

        // Método que vai trabalhar em cima do erro de conexão, sendo resposável por tentar fazer o login
        private void ResolveSignIn()
        {   
            // Verificando se essa conexão tem solução de erro
            if (connectionResult != null && connectionResult.HasResolution)
            {
                try
                {
                    // Se tiver solução, a tela de consentimento de usuário será aberta
                    isConsentScreenOpened = true;
                    // Disparando a Intent de abertura da tela de consentimento de usuário
                    // O retorno dele será pelo método onActivityResult
                    connectionResult.StartResolutionForResult(this, SIGN_IN_CODE);
                }
                catch (SendIntentException e)
                {
                    // Se acontecer algum erro, a tela de consentimento não estará aberta
                    isConsentScreenOpened = false;
                    // Uma nova tentativa de conexão é realizada
                    googleApiClient.Connect();
                }
            }
        }

        // Método responsável por pegar os dados do usuário conectado
        private void getProfileData() {
            var user = PlusClass.PeopleApi.GetCurrentPerson(googleApiClient);
            // String id = user.Id;
            // String profileUrl = user.Url;
            // String language = user.Language
            // Pegando o url da imagem no padrão 50x50 pixels
            // string imageUrl = user.Image.Url;
            // Pegando a url da imagem com tamanho 200x200 pixels
            // imageUrl = imageUrl.substring(0, imageUrl.length() - 2) + 200;

            textViewName.Text = "Name: " + (user.HasDisplayName ? user.DisplayName : "Unknown");
            textViewEmail.Text = "Email: " + PlusClass.AccountApi.GetAccountName(googleApiClient);
            
            switch(user.Gender)
            {
                case 0:
                    textViewGender.Text = "Gender: Male";
                    break;
                case 1:
                    textViewGender.Text = "Gender: Female";
                    break;
                case 3:
                    textViewGender.Text = "Gender: Other";
                    break;
                case 4:
                    textViewGender.Text = "Gender: Unknown";
                    break;
            }

            textViewLanguage.Text = "Language: " + (user.HasLanguage ? user.Language : "Unknown");
            textViewNickName.Text = "Nickname: " + (user.HasNickname ? user.Nickname : "Unknown");
            textViewTagline.Text = "Tagline: " + (user.HasTagline ? user.Tagline : "Unknown");
            textViewAboutMe.Text = "About Me: " + (user.HasAboutMe ? user.AboutMe : "Unknown");
            textViewBirthday.Text = "Birthday: " + (user.HasBirthday ? user.Birthday : "Unknown");
            textViewCurrentLocation.Text = "Current Location: " + (user.HasCurrentLocation ? user.CurrentLocation : "Unknown");
            textViewId.Text = "Id: " + (user.HasId ? user.Id : "Unknown");
            textViewProfileUrl.Text = "Profile url: " + (user.HasUrl ? user.Url : "Unknown");
        }

        protected override void OnStart()
        {
            base.OnStart();

            // Iniciando a conexão com a Gooogle API
            if (googleApiClient != null)
                googleApiClient.Connect();
        }

        protected override void OnStop()
        {
            base.OnStop();

            // Para liberar recurso, a conexão com o Google API é parada
            if (googleApiClient != null && googleApiClient.IsConnected)
                googleApiClient.Disconnect();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == SIGN_IN_CODE)
            {
                // Se a requisição chegou até aqui, a tela de consentimento de usuário já está fechada
                isConsentScreenOpened = false;
                
                // Clicou em cancelar
                if (resultCode != Result.Ok)
                {
                    isSignInClicked = false;
                }

                // Verificar se não está conectando para iniciar a conexão
                if (!googleApiClient.IsConnecting)
                {
                    googleApiClient.Connect();
                }
            }
        }

        // Método de callback responsável por tratar quando o usuário está conectado
        public void OnConnected(Bundle connectionHint)
        {
            // Alterar o estado do botão, visto que o usuário já está conectado
            isSignInClicked = false;
            // TODO: Implementar para a interface de usuário conectado
            // Pegando os dados do usuário
            getProfileData();
            updateUi(true);
        }

        // Método de callback responsável por tratar uma conexão suspensa
        public void OnConnectionSuspended(int cause)
        {
            // Quando a conexão é suspensa, a conexão com o servidor do google é mantida
            googleApiClient.Connect();
            // TODO: Implementar o layout para ir para a tela de login
            updateUi(false);
        }

        // Método de callback responsável por tratar uma conexão que falhou
        public void OnConnectionFailed(ConnectionResult result)
        {
            // Se a folha não tiver solução, um Dialog de erro do Google deve ser exibido
            if(!result.HasResolution)
            {
                GooglePlayServicesUtil.GetErrorDialog(result.ErrorCode, this, 0).Show();
                return;
            }

            if(!isConsentScreenOpened)
            {
                connectionResult = result;

                // Se o botão tiver clicado, tentar uma nova conexão
                if(isSignInClicked)
                {
                    ResolveSignIn();
                }
            }
            updateUi(false);
        }

        // Método responsável por vincular as Views do arquivo XML referente à Activity
        private void bindViewsFromXML() {
            signInButton = FindViewById<SignInButton>(Resource.Id.sign_in_button);
            textViewName = FindViewById<TextView>(Resource.Id.textViewName);
            textViewEmail = FindViewById<TextView>(Resource.Id.textViewEmail);
            textViewStatus = FindViewById<TextView>(Resource.Id.textViewStatus);
            textViewGender = FindViewById<TextView>(Resource.Id.textViewGender);
            textViewLanguage = FindViewById<TextView>(Resource.Id.textViewLanguage);
            textViewNickName = FindViewById<TextView>(Resource.Id.textViewNickName);
            textViewTagline = FindViewById<TextView>(Resource.Id.textViewTagLine);
            textViewAboutMe = FindViewById<TextView>(Resource.Id.textViewAboutMe);
            textViewBirthday = FindViewById<TextView>(Resource.Id.textViewBirthday);
            textViewCurrentLocation = FindViewById<TextView>(Resource.Id.textViewCurrentLocation);
            textViewId = FindViewById<TextView>(Resource.Id.textViewId);
            textViewProfileUrl = FindViewById<TextView>(Resource.Id.textViewProfileUrl);
            buttonSignOut = FindViewById<Button>(Resource.Id.buttonSignOut);
            linearInformations = FindViewById<LinearLayout>(Resource.Id.linearInformations);
            signInButton.Click += SignInButton_Click;
            buttonSignOut.Click += ButtonSignOut_Click;
        }

        private void updateUi(bool isLogged)
        {
            buttonSignOut.Visibility = isLogged ? ViewStates.Visible : ViewStates.Invisible;
            signInButton.Visibility = isLogged ? ViewStates.Invisible : ViewStates.Visible;
            linearInformations.Visibility = isLogged ? ViewStates.Visible : ViewStates.Invisible;
            textViewStatus.Text = isLogged ? "Status: Connected" : "Status: Disconnected";
        }
    }
}

