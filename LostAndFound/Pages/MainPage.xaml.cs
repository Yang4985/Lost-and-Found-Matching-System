using LostAndFound.Models;
using LostAndFound.PageModels;

namespace LostAndFound.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}