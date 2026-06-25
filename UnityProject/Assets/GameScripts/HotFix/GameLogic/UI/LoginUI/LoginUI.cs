using UnityEngine.UI;
using TEngine;
using Log = TEngine.Log;
using UnityEngine.PlayerLoop;
using TMPro;

namespace GameLogic
{
    [Window(UILayer.UI, location: "LoginUI")]
    class LoginUI : UIWindow
    {
        #region 脚本工具生成的代码
        private InputField m_inputAccount = null!;
        private InputField m_inputPassword = null!;
        private Button m_btnLogin = null!;

        private TMP_Dropdown m_dropdown = null!;

        protected override void ScriptGenerator()
        {
            m_inputAccount = FindChildComponent<InputField>("m_inputAccount");
            m_inputPassword = FindChildComponent<InputField>("m_inputPassword");
            m_dropdown = FindChildComponent<TMP_Dropdown>("Dropdown");

            m_btnLogin = FindChildComponent<Button>("m_btnLogin");
            m_btnLogin.onClick.AddListener(OnClickLoginBtn);
            m_dropdown.onValueChanged.AddListener(OnValueChangedDropdown);
        }
        #endregion

        #region 事件
        private void OnClickLoginBtn()
        {
            testOpen = !testOpen;
        }
        #endregion
        private void OnValueChangedDropdown(int index)
        {
            Log.Info("Dropdown clicked, index: {0}", index);
            // m_dropdown.value = index;
        }
        private bool testOpen = false;
        protected override void OnUpdate()
        {
            if(!testOpen) return;
            
            string account = m_inputAccount.text;
                // string password = m_inputPassword.text;
                // Log.Info("Login clicked, account: {0}", account);
            if (m_dropdown.value == 1)
            {
                test(int.Parse(account));
            }
            if (m_dropdown.value == 2)
            {
                Test.test(int.Parse(account));
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
