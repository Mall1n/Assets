using UnityEngine.UI;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

namespace Mall1n.Collar
{
    public class CollarConnect : UdonSharpBehaviour
    {

        [Header("Setting Сhain")]

        [Tooltip("This flag means the length that must be exceeded in order for the object to become attracted")]
        public float lengthMax = 3.0f;
        
        [Tooltip("The number of vertices to be used in the chain")]
        public int stepsLine = 10;

        [Header("'K' Button For Force Escape")]
        public bool ButtonFE = true;

        [UdonSynced] private int idPlayerConnected = -1;
        private int playersCount = 0;
        private VRCPickup vrcPickup;
        private VRCPickup vrcPickupHolder;
        private LineRenderer lineRenderer = null;
        private Transform Holder = null;
        private Transform CollarModel = null;
        private Transform CollarRoot = null;
        private Transform StartChain;
        private Transform EndChain;
        private Transform SpawnHolderPos;
        private bool HoldsPlayer = false;
        private bool SpawnHolder = true;

        private void Start()
        {
            vrcPickup = (VRCPickup)GetComponent(typeof(VRCPickup));
            Holder = transform.parent.Find("Holder");
            vrcPickupHolder = (VRCPickup)Holder.GetComponent(typeof(VRCPickup));
            StartChain = Holder.Find("StartChain");
            CollarModel = transform.Find("Model");
            CollarRoot = transform.parent;
            SpawnHolderPos = CollarModel.Find("SpawnHolder");
            EndChain = CollarModel.Find("EndChain");
            lineRenderer = CollarRoot.GetComponent<LineRenderer>();
            lineRenderer.positionCount = stepsLine;
            lineRenderer.material.mainTextureScale = new Vector2(75 * lengthMax, 1);

            lineRenderer.enabled = false;
            Holder.gameObject.SetActive(false);
        }

        public override void OnPlayerJoined(VRCPlayerApi player) => playersCount++;

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            playersCount--;
            if (player.playerId == idPlayerConnected)
            {
                DeSpawnHolderMethod();
                if (Networking.LocalPlayer.isMaster)
                    idPlayerConnected = -1;
            }
        }

        public override void OnDrop()
        {
            VRCPlayerApi[] players = new VRCPlayerApi[playersCount * 2];
            VRCPlayerApi.GetPlayers(players);

            foreach (VRCPlayerApi item in players)
            {
                if (item == null) continue;

                Vector3 vector = Vector3.Lerp(item.GetBonePosition(HumanBodyBones.Neck), item.GetBonePosition(HumanBodyBones.Head), 0.5f);

                if (Vector3.Distance(vector, transform.position) < 0.1f)
                {
                    idPlayerConnected = item.playerId;
                    break;
                }
                idPlayerConnected = -1;
            }
        }

        private void Update()
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer == null) return;
            VRCPlayerApi connectedPlayer = null;
            if (idPlayerConnected != -1)
                connectedPlayer = VRCPlayerApi.GetPlayerById(idPlayerConnected);

            vrcPickup.pickupable = vrcPickupHolder.pickupable = localPlayer.playerId == idPlayerConnected ? false : true;

            if (connectedPlayer != null && idPlayerConnected != -1 && !vrcPickup.IsHeld)
            {
                Vector3 NeckPos = connectedPlayer.GetBonePosition(HumanBodyBones.Neck);
                //Quaternion NeckRot = connectedPlayer.GetBoneRotation(HumanBodyBones.Neck);
                Vector3 HeadPos = connectedPlayer.GetBonePosition(HumanBodyBones.Head);
                Vector3 LeftPos = connectedPlayer.GetBonePosition(HumanBodyBones.LeftShoulder);
                Vector3 RightPos = connectedPlayer.GetBonePosition(HumanBodyBones.RightShoulder);
                //NeckRot *= Quaternion.AngleAxis(-90, Vector3.right);

                //transform.rotation = Quaternion.LookRotation(Vector3.Normalize(NeckPos - HeadPos));
                CollarModel.rotation = Quaternion.LookRotation(Vector3.Normalize(LeftPos - RightPos), Vector3.Normalize(HeadPos - NeckPos));
                CollarModel.rotation *= Quaternion.AngleAxis(90, Vector3.up);
                CollarModel.rotation *= Quaternion.AngleAxis(5, Vector3.right);
                CollarModel.position = Vector3.Lerp(NeckPos, HeadPos, 0.5f);

                //transform.position = NeckPos;
                //transform.rotation = NeckRot;

                if (SpawnHolder) SpawnHolderMethod();

                transform.position = CollarModel.transform.position;
                transform.rotation = CollarModel.transform.rotation;

                if (connectedPlayer.playerId == idPlayerConnected)
                {
                    Vector3 vector = Holder.position - transform.position;
                    if (vector.magnitude > lengthMax * 3)
                    {
                        connectedPlayer.TeleportTo(Holder.position, Holder.rotation);
                    }
                    else if (vector.magnitude > lengthMax - 0.1f)
                    {
                        float force = (vector.magnitude - lengthMax + 0.5f) * 4.0f;
                        connectedPlayer.SetVelocity(vector.normalized * force);
                    }

                    Chain(vector.magnitude);
                }
                if (connectedPlayer.playerId == localPlayer.playerId && ButtonFE)
                {
                    if (Input.GetKeyDown(KeyCode.K))
                    {
                        Networking.SetOwner(localPlayer, transform.gameObject);
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DeSpawnHolderMethod");
                        idPlayerConnected = -1;
                    }
                }
            }
            else if (vrcPickup.IsHeld)
            {
                Networking.SetOwner(Networking.GetOwner(vrcPickup.gameObject), Holder.gameObject);

                if (HoldsPlayer)
                {
                    DeSpawnHolderMethod();
                }
            }
        }

        private void Chain(float length)
        {
            Vector3 interim;
            interim = (EndChain.position + StartChain.position) / 2;
            float range = lengthMax - length;
            if (range > 0)
                interim -= new Vector3(0, range, 0);

            int count = lineRenderer.positionCount;
            float step = 1.0f / (count - 1);
            for (int i = 0; i < count; i++)
            {
                float t = step * i;
                float x = Point(StartChain.position.x, interim.x, EndChain.position.x, t);
                float y = Point(StartChain.position.y, interim.y, EndChain.position.y, t);
                float z = Point(StartChain.position.z, interim.z, EndChain.position.z, t);

                lineRenderer.SetPosition(i, new Vector3(x, y, z));
            }
        }

        private float Point(float x1, float x2, float x3, float t)
        {
            return (Mathf.Pow((1 - t), 2) * x1) + (2 * (1 - t) * t * x2) + ((t * t) * x3);
        }

        private void SpawnHolderMethod()
        {
            lineRenderer.enabled = true;
            Holder.gameObject.SetActive(true);

            Holder.position = SpawnHolderPos.position;
            Holder.rotation = Quaternion.Euler(0, 0, 0);

            CollarModel.SetParent(CollarRoot);

            SpawnHolder = false;
            HoldsPlayer = true;
        }

        private void DeSpawnHolderMethod()
        {
            lineRenderer.enabled = false;
            Holder.gameObject.SetActive(false);

            CollarModel.SetParent(transform);

            CollarModel.transform.localPosition = Vector3.zero;
            CollarModel.transform.localRotation = Quaternion.Euler(0, 0, 0);

            HoldsPlayer = false;
            SpawnHolder = true;
        }
    }
}

