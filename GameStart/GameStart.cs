using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HMFW
{


    public class GameStart : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

            GameStateManager.Instance.ChangeState<InitState>();
        }


        void Update()
        {

        }
    }
}