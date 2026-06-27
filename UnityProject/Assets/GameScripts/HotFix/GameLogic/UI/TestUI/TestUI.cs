using UnityEngine.UI;
using TEngine;
using Log = TEngine.Log;
using UnityEngine.PlayerLoop;

namespace GameLogic
{
    [Window(UILayer.UI, location: "TestUI")]
    class TestUI : UIWindow
    {
        #region 脚本工具生成的代码
        private InputField m_inputAccount = null!;
        private InputField m_inputPassword = null!;
        private Button m_btnLogin = null!;

        protected override void ScriptGenerator()
        {
            // m_inputAccount = FindChildComponent<InputField>("m_inputAccount");
            // m_inputPassword = FindChildComponent<InputField>("m_inputPassword");
            // m_btnLogin = FindChildComponent<Button>("m_btnLogin");
            // m_btnLogin.onClick.RemoveAllListeners();
            // m_btnLogin.onClick.AddListener(OnClickLoginBtn);
        }
        #endregion

        #region 事件
        private void OnClickLoginBtn()
        {
            testOpen = !testOpen;
        }
        #endregion
        private bool testOpen = false;
        protected override void OnUpdate()
        {
            if (testOpen)
            {
                string account = m_inputAccount.text;
                string password = m_inputPassword.text;
                Log.Info("Login clicked, account: {0}", account);
                // var len = int.Parse(account);
                // for (int i = 0; i < len; i++)
                //     {
                        test(int.Parse(password));
                    // }
            }
        }

        void test(int len)
        {
            var cnt = 0;
            for (int i = 0; i < len; i++)
            {
                cnt += i;
            }
            //Log.Info("Login clicked, cnt: {0}", cnt);
        }
    }

}
