using UnityEngine;
using CardDuel.Networking.Netcode;
using System;
namespace CardDuel.Gameplay.Turn
{
    public class ServerTurnTimer : MonoBehaviour
    {
        private float _remaining;
        private bool _running;

        public float RemainingTime
        {
            get { return _remaining; }
             set { _remaining = value; }
        }
        public void StartTimer(float seconds)
        {
            _remaining = seconds;
            _running = true;
        }

        public void Pause()
        {
            _running = false;
        }


        private void Update()
        {
            if (!_running || !NetworkGameController.Instance.IsServer) return;

            _remaining -= Time.deltaTime;

            if (_remaining <= 0)
            {
                _running = false;
                // NetworkGameController.Instance.ForceEndTurn();
            }
        }

        internal void Resume()
        {
            _running = true;
        }
    }
}
