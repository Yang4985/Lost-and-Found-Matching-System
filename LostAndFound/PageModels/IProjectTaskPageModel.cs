using CommunityToolkit.Mvvm.Input;
using LostAndFound.Models;

namespace LostAndFound.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}