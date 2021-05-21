using ICities;
using UnityEngine;

namespace FishIndustryEnhanced
{
   public class LoadingExtension : LoadingExtensionBase
    {
        private static GameObject _gameObject;
        

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
            {
                return;
            }
            if (!_gameObject)
            {
                return;
            }
            _gameObject = new GameObject("Warehouse");
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
    }
}
