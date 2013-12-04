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
    public partial class CommentsPage : PhoneApplicationPage
    {
        string url;
        bool isToEnd = false;
        bool isRefreshComments = false;

        public CommentsPage()
        {
            InitializeComponent();

            DataContext = App.ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.TryGetValue("url", out url))
            {
                RefreshComments(true);
            }
            string commentCount = "0";
            if (NavigationContext.QueryString.TryGetValue("comment_count", out commentCount))
            {
                CommentCount.Text = "共 " + commentCount + " 条评论";
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            App.ViewModel.comments.Clear();
        }

        private async void RefreshComments(bool firstPage)
        {
            try
            {
                isRefreshComments = true;
                ZhihuAPI.Comments comments = new ZhihuAPI.Comments(url);
                int count;
                if (firstPage)
                {
                    count = await comments.GetFirstPage();
                    App.ViewModel.comments.Clear();
                }
                else
                    count = await comments.GetNextPage();
                if (count <= 0)
                    isToEnd = true;
                foreach (ZhihuAPI.Comments.Comment item in comments.comment)
                {
                    string showTitle = item.author.name;
                    if (item.isAuthor)
                        showTitle = showTitle + " （作者）";
                    string showContent = item.content;
                    string showTime = "";
                    DateTime dt = new DateTime(1970, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc);
                    dt = dt.AddSeconds((double)item.createdTime);
                    if (dt.Date == DateTime.Today)
                        showTime = dt.ToString("HH:mm");
                    else
                        showTime = dt.ToString("yyyy-MM-dd");
                    App.ViewModel.comments.Add(new ViewModels.CommentsViewModel()
                    {
                        showTitle = showTitle,
                        showContent = showContent,
                        showTime = showTime,
                        comment = item,
                    });
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                isRefreshComments = false;
            }
        }

        #region ScrollBar event
        private void LongListSelector_Loaded(object sender, RoutedEventArgs e)
        {
            List<ScrollBar> scrollBarList = GetVisualChildCollection<ScrollBar>(LongListSelector_Comments);

            foreach (ScrollBar scrollBar in scrollBarList)
            {

                if (scrollBar.Orientation == System.Windows.Controls.Orientation.Vertical)
                {
                    scrollBar.ValueChanged += new RoutedPropertyChangedEventHandler<double>(ScrollBar_ValueChanged);
                }
                else
                {

                }
            }
        }

        private void ScrollBar_ValueChanged(object sender, RoutedEventArgs e)
        {
            ScrollBar scrollBar = (ScrollBar)sender;
            if (scrollBar.Maximum - scrollBar.Value <= 1000 && scrollBar.Maximum - scrollBar.Value > 0 && !isToEnd && ! isRefreshComments)
                RefreshComments(false);
        }

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
    }
}