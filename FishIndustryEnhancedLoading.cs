using ICities;
using UnityEngine;

namespace FishIndustryEnhanced
{
   public class LoadingExtension : LoadingExtensionBase
    {
        private static GameObject _gameObject;
        
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            if (loading.currentMode != AppMode.Game)
            {
                return;
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
            {
                return;
            }
            if (_gameObject != null)
            {
                return;
            }
            _gameObject = new GameObject("CargoFerries");
            _gameObject.AddComponent<WarehousePanelExtender>();
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            if (_gameObject == null)
            {
                return;
            }
            Object.Destroy(_gameObject);
            _gameObject = null;
        } 

        public override void OnReleased()
        {
            base.OnReleased();
        }
    }
}
