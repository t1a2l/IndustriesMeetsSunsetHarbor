using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.AI
{
    public class RestaurantDeliveryPersonAI : ServicePersonAI
    {
        public bool m_hasFood;

        public override void InitializeAI()
        {
            base.InitializeAI();
        }

        public override void ReleaseAI()
        {
            base.ReleaseAI();
        }

        public override Color GetColor(ushort instanceID, ref CitizenInstance data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            if (infoMode == InfoManager.InfoMode.Happiness)
            {
                return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
            }
            return base.GetColor(instanceID, ref data, infoMode, subInfoMode);
        }

        public override string GetLocalizedStatus(ushort instanceID, ref CitizenInstance data, out InstanceID target)
        {
            if ((data.m_flags & (CitizenInstance.Flags.Blown | CitizenInstance.Flags.Floating)) != 0)
            {
                target = InstanceID.Empty;
                return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_CONFUSED");
            }
            target = InstanceID.Empty;
            return "Delivering food";
        }

        public override void CreateInstance(ushort instanceID, ref CitizenInstance data)
        {
            base.CreateInstance(instanceID, ref data);
        }

        public override void LoadInstance(ushort instanceID, ref CitizenInstance data)
        {
            base.LoadInstance(instanceID, ref data);
            if (data.m_targetBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
            }
        }

        public override void ReleaseInstance(ushort instanceID, ref CitizenInstance data)
        {
            if (data.m_citizen != 0)
            {
                CitizenManager instance = Singleton<CitizenManager>.instance;
                instance.m_citizens.m_buffer[data.m_citizen].m_instance = 0;
                instance.ReleaseCitizen(data.m_citizen);
                data.m_citizen = 0u;
            }
            base.ReleaseInstance(instanceID, ref data);
        }

        public override void SimulationStep(ushort instanceID, ref CitizenInstance data, Vector3 physicsLodRefPos)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            if (instance.m_citizens.m_buffer[data.m_citizen].m_vehicle == 0)
            {
                instance.ReleaseCitizenInstance(instanceID);
                return;
            }
            if (data.m_waitCounter < byte.MaxValue)
            {
                data.m_waitCounter++;
            }
            base.SimulationStep(instanceID, ref data, physicsLodRefPos);
        }

        public override void SetTarget(ushort instanceID, ref CitizenInstance data, ushort targetIndex, bool targetIsNode)
        {
            if (targetIsNode)
            {
                targetIndex = 0;
            }
            if (targetIndex == data.m_targetBuilding)
            {
                return;
            }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if (data.m_targetBuilding != 0)
            {
                instance.m_buildings.m_buffer[data.m_targetBuilding].RemoveTargetCitizen(instanceID, ref data);
            }
            data.m_targetBuilding = targetIndex;
            if (data.m_targetBuilding != 0)
            {
                instance.m_buildings.m_buffer[data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
                data.m_waitCounter = 0;
                return;
            }
            if ((data.m_flags & CitizenInstance.Flags.Character) == 0)
            {
                Vector3 vector = GetVehicleEnterPosition(instanceID, ref data, 1f);
                Vector3 position = data.GetLastFrameData().m_position;
                Vector3 forward = vector - position;
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.01f)
                {
                    CitizenInstance.Frame frame = default(CitizenInstance.Frame);
                    frame.m_position = position;
                    frame.m_rotation = Quaternion.LookRotation(forward);
                    data.m_frame0 = frame;
                    data.m_frame1 = frame;
                    data.m_frame2 = frame;
                    data.m_frame3 = frame;
                }
            }
            data.m_flags |= CitizenInstance.Flags.EnteringVehicle;
            data.Spawn(instanceID);
        }

        protected override Vector4 GetVehicleEnterPosition(ushort instanceID, ref CitizenInstance citizenData, float minSqrDistance)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            VehicleManager instance2 = Singleton<VehicleManager>.instance;
            uint citizen = citizenData.m_citizen;
            if (citizen != 0)
            {
                ushort vehicle = instance.m_citizens.m_buffer[citizen].m_vehicle;
                if (vehicle != 0)
                {
                    Vector4 result = instance2.m_vehicles.m_buffer[vehicle].GetClosestDoorPosition(citizenData.m_targetPos, (!m_hasFood) ? VehicleInfo.DoorType.Enter : VehicleInfo.DoorType.Load);
                    result.w = citizenData.m_targetPos.w;
                    return result;
                }
            }
            return citizenData.m_targetPos;
        }

        protected override void GetBuildingTargetPosition(ushort instanceID, ref CitizenInstance citizenData, float minSqrDistance)
        {
            if (citizenData.m_targetBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingInfo info = instance.m_buildings.m_buffer[citizenData.m_targetBuilding].Info;
                Randomizer randomizer = new Randomizer(instanceID);
                info.m_buildingAI.CalculateUnspawnPosition(citizenData.m_targetBuilding, ref instance.m_buildings.m_buffer[citizenData.m_targetBuilding], ref randomizer, m_info, instanceID, out var position, out var _, out var direction, out var _);
                citizenData.m_targetPos = new Vector4(position.x, position.y, position.z, 1f);
                citizenData.m_targetDir = direction;
            }
            else
            {
                citizenData.m_targetDir = Vector2.zero;
            }
        }

        protected override bool EnterVehicle(ushort instanceID, ref CitizenInstance citizenData)
        {
            citizenData.Unspawn(instanceID);
            uint citizen = citizenData.m_citizen;
            if (citizen != 0)
            {
                Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizen].SetVehicle(citizen, 0, 0u);
            }
            return true;
        }

        protected override bool ArriveAtTarget(ushort instanceID, ref CitizenInstance citizenData)
        {
            citizenData.Unspawn(instanceID);
            return false;
        }

        protected override void ArriveAtDestination(ushort instanceID, ref CitizenInstance citizenData, bool success)
        {
        }
    }
}
