using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Zhihu
{
    public partial class MainPage : PhoneApplicationPage
    {
        bool isRefreshHotAnswers = false;
        bool alreadyRefreshHotAnswers = false;
        bool isRefreshTimeline = false;
        bool alreadyRefreshTimeline = false;


        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            Application.Current.Terminate();
        }

        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            // 将 listbox 控件的数据上下文设置为示例数据
            DataContext = App.ViewModel;
            ApplicationBar = (ApplicationBar)Resources["AppBar0"];
        }

        // 为 ViewModel 项加载数据
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
                ZhihuAPI.HotAnswers api = new ZhihuAPI.HotAnswers();
                RefreshTimeline(true);
                RefreshHotAnswers(true);
            }
        }

        #region 刷新最新动态
        private async void RefreshTimeline(bool firstPage)
        {
            try
            {
                if (!alreadyRefreshTimeline)
                    TimelineProgressBar.Visibility = System.Windows.Visibility.Visible;
                isRefreshTimeline = true;

                ZhihuAPI.Timeline timeline = new ZhihuAPI.Timeline();
                if (firstPage)
                {
                    await timeline.GetFirstPage();
                    App.ViewModel.timeline.Clear();
                }
                else
                {
                    await timeline.GetNextPage();
                }

                foreach (ZhihuAPI.Timeline.Feed item in timeline.timeline)
                {
                    string showTitle;
                    string showContent;
                    string showName;
                    string showVoteupCount;
                    System.Windows.Visibility visibility;

                    switch (item.verb)
                    {
                        case "ANSWER_CREATE":
                            showTitle = item.target.question.title;
                            showContent = item.target.excerpt;
                            showName = item.actors[0].name + " 回答了该问题";
                            showVoteupCount = item.target.voteupCount.ToString();
                            visibility = System.Windows.Visibility.Visible;
                            break;
                        case "ANSWER_VOTE_UP":
                            showTitle = item.target.question.title;
                            showContent = item.target.excerpt;
                            showName = item.actors[0].name + " 赞同该回答";
                            showVoteupCount = item.target.voteupCount.ToString();
                            visibility = System.Windows.Visibility.Visible;
                            break;
                        case "QUESTION_FOLLOW":
                            showTitle = item.target.title;
                            showContent = "";
                            showName = item.actors[0].name + " 关注该问题";
                            showVoteupCount = "0";
                            visibility = System.Windows.Visibility.Collapsed;
                            break;
                        case "QUESTION_CREATE":
                            showTitle = item.target.title;
                            showContent = "";
                            showName = item.actors[0].name + " 提了一个问题";
                            showVoteupCount = "0";
                            visibility = System.Windows.Visibility.Collapsed;
                            break;
                        default:
                            showTitle = item.target.question.title;
                            showContent = "未定义";
                            showName = item.verb;
                            showVoteupCount = "0";
                            visibility = System.Windows.Visibility.Collapsed;
                            break;
                    }

                    App.ViewModel.timeline.Add(new ViewModels.TimelineViewModel()
                    {
                        feed = item,
                        showTitle = showTitle,
                        showContent = showContent,
                        showName = showName,
                        showVoteupCount = showVoteupCount,
                        visibility = visibility,
                    });
                }
                alreadyRefreshTimeline = true;
            }
            catch
            {

            }
            finally
            {
                isRefreshTimeline = false;
                TimelineProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        #endregion

        #region 刷新热门问答
        private async void RefreshHotAnswers(bool firstPage)
        {
            try
            {
                if (!alreadyRefreshHotAnswers)
                    HotAnswersProgressBar.Visibility = System.Windows.Visibility.Visible;
                isRefreshHotAnswers = true;

                ZhihuAPI.HotAnswers hotAnswers = new ZhihuAPI.HotAnswers();
                if (firstPage)
                {
                    await hotAnswers.GetFirstPage();
                    App.ViewModel.hotAnswers.Clear();
                }
                else
                {
                    await hotAnswers.GetNextPage();
                }
                Debug.WriteLine(hotAnswers.hotAnswers.Count);
                foreach (ZhihuAPI.HotAnswers.Answer item in hotAnswers.hotAnswers)
                {
                    App.ViewModel.hotAnswers.Add(new ViewModels.HotAnswersViewModel()
                    {
                        authorHeadline = item.author.headline,
                        authorAvatarUrl = item.author.avatarUrl,
                        authorName = item.author.name,
                        authorUrl = item.author.url,
                        authorGender = item.author.gender,
                        authorType = item.author.type,
                        authorId = item.author.id,
                        questionUrl = item.question.url,
                        questionType = item.question.type,
                        questionId = item.question.id,
                        questionTitle = item.question.title,
                        url = item.url,
                        excerpt = item.excerpt,
                        updatedTime = item.updatedTime,
                        commentCount = item.commentCount,
                        type = item.type,
                        id = item.id,
                        voteupCount = item.voteupCount
                    });
                }
                alreadyRefreshHotAnswers = true;
            }
            catch
            {

            }
            finally
            {
                isRefreshHotAnswers = false;
                HotAnswersProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        #endregion

        #region ScrollBar event
        // Timeline
        private void LongListSelector_Timeline_Loaded(object sender, RoutedEventArgs e)
        {
            List<ScrollBar> scrollBarList = GetVisualChildCollection<ScrollBar>(LongListSelector_Timeline);

            foreach (ScrollBar scrollBar in scrollBarList)
            {

                if (scrollBar.Orientation == System.Windows.Controls.Orientation.Vertical)
                {
                    scrollBar.ValueChanged += new RoutedPropertyChangedEventHandler<double>(Timeline_ScrollBar_ValueChanged);
                }
                else
                {

                }
            }
        }

        private void Timeline_ScrollBar_ValueChanged(object sender, RoutedEventArgs e)
        {
            ScrollBar scrollBar = (ScrollBar)sender;
            if (scrollBar.Maximum - scrollBar.Value <= 1000 && scrollBar.Maximum - scrollBar.Value > 0 && !isRefreshTimeline)
                RefreshTimeline(false);
        }
        // end of Timeline

        // HotAnswers
        private void LongListSelector_HotAnswers_Loaded(object sender, RoutedEventArgs e)
        {
            List<ScrollBar> scrollBarList = GetVisualChildCollection<ScrollBar>(LongListSelector_HotAnswers);

            foreach (ScrollBar scrollBar in scrollBarList)
            {

                if (scrollBar.Orientation == System.Windows.Controls.Orientation.Vertical)
                {
                    scrollBar.ValueChanged += new RoutedPropertyChangedEventHandler<double>(HotAnswers_ScrollBar_ValueChanged);
                }
                else
                {

                }
            }
        }

        private void HotAnswers_ScrollBar_ValueChanged(object sender, RoutedEventArgs e)
        {
            ScrollBar scrollBar = (ScrollBar)sender;
            if (scrollBar.Maximum - scrollBar.Value <= 1000 && scrollBar.Maximum - scrollBar.Value > 0 && !isRefreshHotAnswers)
                RefreshHotAnswers(false);
        }
        // end of HotAnswers

        public static List<T> GetVisualChildCollection<T>(object parent) where T : UIElement
        {
            List<T> visualCollection = new List<T>();
            GetVisualChildCollection(parent as DependencyObject, visualCollection);
            return visualCollection;
        }


        private static void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection) where T : UIElement
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    visualCollection.Add(child as T);
                }
                else if (child != null)
                {
                    GetVisualChildCollection(child, visualCollection);
                }
            }
        }
        #endregion

        #region other Controls event
        private void QuestionTitle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
        }

        private void TimelineQuestionDetail_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ViewModels.TimelineViewModel feed = ((sender as TextBlock).DataContext as ViewModels.TimelineViewModel);
            NavigationService.Navigate(new Uri("/AnswerDetailPage.xaml?url=" + feed.feed.target.url, UriKind.Relative));
        }

        private void HotAnswersQuestionDetail_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ViewModels.HotAnswersViewModel answers = (((sender as TextBlock)).DataContext as ViewModels.HotAnswersViewModel);
            NavigationService.Navigate(new Uri("/AnswerDetailPage.xaml?url=" + answers.url, UriKind.Relative));
        }

        private void Panorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (panorama.SelectedIndex)
            {
                case 0:
                    ApplicationBar = (ApplicationBar)Resources["AppBar0"];
                    break;
                case 1:
                    ApplicationBar = (ApplicationBar)Resources["AppBar1"];
                    break;
                case 2:
                    ApplicationBar = (ApplicationBar)Resources["AppBar2"];
                    break;
            }
        }
        #endregion

        #region Application Bar event
        private void RefreshHotAnswers_Click(object sender, EventArgs e)
        {
            if (!isRefreshHotAnswers)
            {
                alreadyRefreshHotAnswers = false;
                RefreshHotAnswers(true);
            }
        }
        private void Search_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SearchPage.xaml", UriKind.Relative));
        }

        private void RefreshTimeline_Click(object sender, EventArgs e)
        {
            if (!isRefreshTimeline)
            {
                alreadyRefreshTimeline = false;
                RefreshTimeline(true);
            }
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }
        #endregion

        #region 动画

        #endregion
    }
}