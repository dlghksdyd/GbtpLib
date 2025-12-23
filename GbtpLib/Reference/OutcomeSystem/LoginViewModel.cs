using OutcomeSystem.Design.Popup;
using OutcomeSystem.Repository;
using MsSql.Utility;
using MsSqlProcessor.MsSql;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OutcomeSystem.Design
{
    public class LoginViewModel : BindableBase
    {
        private string _userId = string.Empty;
        public string UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public void Login()
        {
            var selectQueryBuilder = new MssqlSelectQueryBuilder();
            selectQueryBuilder.AddAllColumns<MST_USER_INFO>();
            selectQueryBuilder.AddWhere(nameof(MST_USER_INFO), nameof(MST_USER_INFO.USER_ID), UserId);
            selectQueryBuilder.AddWhere(nameof(MST_USER_INFO), nameof(MST_USER_INFO.PWD), Password);
            var users = MssqlBasic.Select<MST_USER_INFO>(nameof(MST_USER_INFO), selectQueryBuilder);

            if (users.Count > 0)
            {
                RepoUser.UserId = UserId;
                RepoUser.Password = Password;
                RepoUser.IsAdmin = (users.First().USER_GROUP_CD == "ADMN");
            }
            else
            {
                new PopupWarningViewModel()
                {
                    Title = "로그인 실패",
                    Message = "아이디 또는 비밀번호가 잘못되었습니다.",
                    CancelButtonVisibility = Visibility.Collapsed
                }.Open();
            }
        }
    }
}
