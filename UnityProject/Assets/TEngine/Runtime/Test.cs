using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    public class Test
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public static void test(int len)
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
