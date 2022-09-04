using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript {
    partial class Program : MyGridProgram {
        //-----------ScriptConfig-----------
        public string ShipName = "AShipTest"; //ADD THIS NAME TO YOUR SHIP'S GRID NAME
        public string ConnectorName = "Connector"; //ADD THE SHIPNAME IN THE CONNECTOR YOU WILL USE TO UNLOAD/RECHARGE (EX:AShipTestConnector)
        public float MinBatteryPower = 0.2f; //REPRESENTS THE MINIMUM PERCENTAGE OF BATTERIES (EX: 0.2 = 20%)
        public bool ShowBatteryInfo = true;

        //----------------------------------

        MyShip thisShip;

        public Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        void Main(string argument, UpdateType update) {
            if(thisShip == null) {
                thisShip = new MyShip(ShipName, this);
            }
            if(update != 0) {
                if(argument != "") {
                    switch(argument.ToLower()) {
                        case "":

                            break;
                    }
                }
                thisShip.Update();
            }
        }
        public class MyShip {
            internal static Program ThisProgram;
            public string Name { get; private set; }
            public string TextStatus { get; set; }
            private MyBattery BatteryBlock;
            private MyConnector ConnectorBlock;
            private MyCargo CargoBlock;
            private MyTextPanel TextPanelBlock;
            public MyShip(string name, Program program) {
                Name = name;
                ThisProgram = program;
                InitShipSystems();
            }
            private void InitShipSystems() {
                BatteryBlock = new MyBattery(this);
                ConnectorBlock = new MyConnector(this);
                CargoBlock = new MyCargo(this);
                TextPanelBlock = new MyTextPanel(this);
            }
            public void Update() {
                BatteryBlock.Update();
                ConnectorBlock.Update();
                TextPanelBlock.Update();
            }
            private string RenameBlock(string block, int count) {
                if(count < 10) {
                    return $"{Name}{block}00{count}";
                }
                else if(count > 9 && count < 100) {
                    return $"{Name}{block}0{count}";
                }
                else
                    return $"{Name}{block}{count}";
            }
            public class MyTextPanel {
                private MyShip myShip;
                List<IMyTerminalBlock> TextPanelList = new List<IMyTerminalBlock>();
                public string StatusPanelName { get; private set; }
                public MyTextPanel(MyShip ms) {
                    myShip = ms;
                    StatusPanelName = "TP_STATUS";
                    List<IMyTerminalBlock> auxList = new List<IMyTerminalBlock>();
                    ThisProgram.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(auxList);
                    TextPanelList.Clear();
                    foreach(var x in auxList) {
                        var textPanel = x as IMyTextPanel;
                        if(textPanel != null) {
                            if(textPanel.CubeGrid.CustomName == myShip.Name) {
                                TextPanelList.Add(textPanel);
                            }
                        }
                    }
                }
                public void Update() {
                    StatusTP();
                }
                public void StatusTP() {
                    string textoutput = "";
                    foreach(var x in TextPanelList) {
                        var panel = x as IMyTextPanel;
                        if(panel != null) {
                            if(panel.CubeGrid.CustomName == myShip.Name && panel.CustomName == myShip.Name + StatusPanelName) {
                                panel.ContentType = (ContentType)1;
                                panel.FontColor = Color.Yellow;
                                panel.Alignment = (TextAlignment)0;
                                panel.FontSize = 1.25f;
                                textoutput = "SHIP STATUS\n";
                                if(panel.DefinitionDisplayNameText != "Wide LCD Panel") {
                                    if(ThisProgram.ShowBatteryInfo) {
                                        textoutput += "\nBattery Power:                     ";
                                        textoutput += ((myShip.BatteryBlock.StoredPower / myShip.BatteryBlock.MaxPower) * 100).ToString("F2") + "%\n";
                                        if(myShip.BatteryBlock.IsCharging) {
                                            textoutput += "Battery Status:          RECHARGING\n";
                                        }
                                        else {
                                            textoutput += "Battery Status:                      AUTO\n";
                                        }
                                    }
                                }
                                else {
                                    if(ThisProgram.ShowBatteryInfo) {
                                        textoutput += "\nBattery Power:                                                                             ";
                                        textoutput += ((myShip.BatteryBlock.StoredPower / myShip.BatteryBlock.MaxPower) * 100).ToString("F2") + "%\n";
                                        if(myShip.BatteryBlock.IsCharging) {
                                            textoutput += "Battery Status                                                                    RECHARGING\n";
                                        }
                                        else {
                                            textoutput += "Battery Status:                                                                               AUTO\n";
                                        }
                                    }
                                }
                                panel.WriteText(textoutput);
                            }
                        }
                    }
                }
            }
            public class MyCargo {
                List<IMyTerminalBlock> ContainerList = new List<IMyTerminalBlock>();
                private MyShip myShip;
                public string Name { get; private set; }
                public int MaxVolume { get; private set; }
                public int CurrentVolume { get; private set; }
                public int CurrentMass { get; private set; }
                public int CoAmount { get; private set; }
                public int AuAmount { get; private set; }
                public int IceAmount { get; private set; }
                public int FeAmount { get; private set; }
                public int MgAmount { get; private set; }
                public int NiAmount { get; private set; }
                public int PtAmount { get; private set; }
                public int SiAmount { get; private set; }
                public int AgAmount { get; private set; }
                public int StoneAmount { get; private set; }
                public int UAmount { get; private set; }
                public MyCargo(MyShip ms) {
                    myShip = ms;
                    Name = "Container";
                    List<IMyTerminalBlock> auxList = new List<IMyTerminalBlock>();
                    ThisProgram.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(auxList);
                    ContainerList.Clear();
                    int auxCount = 1;
                    foreach(var x in auxList) {
                        var container = x;
                        if(container != null) {
                            if(container.CubeGrid.CustomName == myShip.Name) {
                                if(container is IMyCargoContainer) {
                                    container.CustomName = myShip.RenameBlock(Name, auxCount);
                                    auxCount++;
                                }
                                if(container is IMyCargoContainer | container is IMyShipDrill | container is IMyShipConnector | container is IMyCockpit) {
                                    MaxVolume += (int)container.GetInventory(0).MaxVolume;
                                    CurrentVolume += (int)container.GetInventory(0).CurrentVolume;
                                    CurrentMass += (int)container.GetInventory(0).CurrentVolume;
                                    ContainerList.Add(container);
                                }
                            }
                        }
                    }
                }
                public void Update() {
                    CurrentVolume = 0;
                    CurrentMass = 0;
                    CoAmount = 0;
                    AuAmount = 0;
                    IceAmount = 0;
                    FeAmount = 0;
                    MgAmount = 0;
                    NiAmount = 0;
                    PtAmount = 0;
                    SiAmount = 0;
                    AgAmount = 0;
                    StoneAmount = 0;
                    UAmount = 0;
                    foreach(var container in ContainerList) {
                        if(container != null) {
                            CurrentVolume += (int)container.GetInventory(0).CurrentVolume;
                            CurrentMass += (int)container.GetInventory(0).CurrentMass;
                            var itemsList = new List<MyInventoryItem>();
                            container.GetInventory(0).GetItems(itemsList);
                            foreach(var item in itemsList) {
                                if(item != null) {
                                    switch(item.Type.SubtypeId) {
                                        case "Cobalt":
                                            CoAmount += (int)item.Amount;
                                            break;
                                        case "Gold":
                                            AuAmount += (int)item.Amount;
                                            break;
                                        case "Ice":
                                            IceAmount += (int)item.Amount;
                                            break;
                                        case "Iron":
                                            FeAmount += (int)item.Amount;
                                            break;
                                        case "Magnesium":
                                            MgAmount += (int)item.Amount;
                                            break;
                                        case "Nickel":
                                            NiAmount += (int)item.Amount;
                                            break;
                                        case "Platinum":
                                            PtAmount += (int)item.Amount;
                                            break;
                                        case "Silicon":
                                            SiAmount += (int)item.Amount;
                                            break;
                                        case "Silver":
                                            AgAmount += (int)item.Amount;
                                            break;
                                        case "Stone":
                                            StoneAmount += (int)item.Amount;
                                            break;
                                        case "Uranium":
                                            UAmount += (int)item.Amount;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            public class MyBattery {
                private MyShip myShip;
                public List<IMyTerminalBlock> BatteryList = new List<IMyTerminalBlock>();
                public string Name { get; private set; }
                public float StoredPower { get; private set; }
                public float MaxPower { get; private set; }
                public float MinPower { get; private set; }
                public bool LowPower { get; private set; }
                public bool IsCharging { get; private set; }
                public MyBattery(MyShip ms) {
                    myShip = ms;
                    Name = "Battery";
                    StoredPower = 0f;
                    MaxPower = 0f;
                    MinPower = ThisProgram.MinBatteryPower;
                    LowPower = false;
                    List<IMyTerminalBlock> auxList = new List<IMyTerminalBlock>();
                    ThisProgram.GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(auxList);
                    BatteryList.Clear();
                    int auxcount = 1;
                    foreach(var x in auxList) {
                        var battery = x as IMyBatteryBlock;
                        if(battery != null) {
                            if(battery.CubeGrid.CustomName == myShip.Name) {
                                battery.CustomName = myShip.RenameBlock(Name, auxcount);
                                StoredPower += battery.CurrentStoredPower;
                                MaxPower += battery.MaxStoredPower;
                                BatteryList.Add(battery);
                                auxcount++;
                            }
                        }
                    }
                }
                public void Recharge(bool toggleMode) {
                    foreach(var x in BatteryList) {
                        var battery = x as IMyBatteryBlock;
                        if(battery != null) {
                            if(toggleMode) {
                                battery.ChargeMode = ChargeMode.Recharge;
                            }
                            else {
                                battery.ChargeMode = ChargeMode.Auto;
                            }
                            IsCharging = toggleMode;
                        }
                    }
                }
                public void Update() {
                    StoredPower = 0f;
                    foreach(var x in BatteryList) {
                        var battery = x as IMyBatteryBlock;
                        if(battery != null) {
                            Recharge((myShip.ConnectorBlock.Connector.Status == MyShipConnectorStatus.Connected));
                            StoredPower += battery.CurrentStoredPower;
                        }
                        LowPower = (StoredPower / MaxPower) < MinPower;
                    }
                }
            }
            public class MyConnector {
                private MyShip myShip;
                public IMyShipConnector Connector;
                public bool Connectable { get; private set; }
                public bool Connected { get; private set; }
                public string Name { get; private set; }
                public MyConnector(MyShip ms) {
                    myShip = ms;
                    Name = ThisProgram.ConnectorName;
                    Connector = ThisProgram.GridTerminalSystem.GetBlockWithName(myShip.Name + Name) as IMyShipConnector;
                }
                public void Update() {
                    if(Connector == null) {
                        Connector = ThisProgram.GridTerminalSystem.GetBlockGroupWithName(myShip.Name + Name) as IMyShipConnector;
                    }
                    if(Connector.Status == MyShipConnectorStatus.Connectable) {
                        Connectable = true;
                    }
                    else {
                        Connectable = false;
                    }
                    if(Connector.Status == MyShipConnectorStatus.Connected) {
                        Connected = true;
                    }
                    else {
                        Connected = false;
                    }
                }
            }
        }
    }
}