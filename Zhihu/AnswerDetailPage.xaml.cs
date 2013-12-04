using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Diagnostics;
using System.Windows.Documents;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;

namespace Zhihu
{
    public partial class AnswerDetailPage : PhoneApplicationPage
    {
        string url;
        ZhihuAPI.Answers answer = new ZhihuAPI.Answers();

        public AnswerDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.TryGetValue("url", out url))
            {
                RefreshAnswer();
            }
            else
            {

            }
        }

        public async void RefreshAnswer()
        {
            try
            {
                await answer.GetAnswerDetail(url);
                ZhihuAPI.HTMLOutput htmlOutput = new ZhihuAPI.HTMLOutput(answer.answerDetail.content);
                AnswerContent.NavigateToString(htmlOutput.OutHtml);

                IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
                if (file.FileExists("AnswerDetail.html"))
                {
                    file.DeleteFile("AnswerDetail.html");
                }
                using (BinaryWriter bw = new BinaryWriter(file.CreateFile("AnswerDetail.html")))
                {
                    byte[] data = Encoding.UTF8.GetBytes(htmlOutput.OutHtml);
                    bw.Write(data);
                    bw.Close();
                }
                AnswerContent.Navigate(new Uri("AnswerDetail.html", UriKind.Relative));
            }
            catch
            {

            }
        }

        private void AnswerContent_LoadCompleted(object sender, NavigationEventArgs e)
        {
            AnswerContent.Opacity = 1;
        }

        private void Comments_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/CommentsPage.xaml?url=" + url + "&comment_count=" + answer.answerDetail.commentCount, UriKind.Relative));
        }
    }
}