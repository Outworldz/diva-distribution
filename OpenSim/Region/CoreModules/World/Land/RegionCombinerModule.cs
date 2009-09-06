/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Nini.Config;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

namespace OpenSim.Region.CoreModules.World.Land
{
    public class RegionCombinerModule : ISharedRegionModule
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string Name { get
        {
            return "RegionCombinerModule";
        } }
        public Type ReplaceableInterface
        {
            get { return null; }
        }

        private Dictionary<UUID, RegionConnections> m_regions = new Dictionary<UUID, RegionConnections>();
        private bool enabledYN = false;
        private Dictionary<UUID, Scene> m_startingScenes = new Dictionary<UUID, Scene>();

        public void Initialise(IConfigSource source)
        {
         
            IConfig myConfig = source.Configs["Startup"];
            enabledYN = myConfig.GetBoolean("CombineContiguousRegions", false);
            //enabledYN = true;
           
        }

        public void Close()
        {
            
        }

        public void AddRegion(Scene scene)
        {
           
            if (!enabledYN)
                return;

            lock (m_startingScenes)
                m_startingScenes.Add(scene.RegionInfo.originRegionID, scene);

            Border northBorder = new Border();
            northBorder.BorderLine = new Vector3(0, (int)Constants.RegionSize, (int)Constants.RegionSize);  //<---
            northBorder.CrossDirection = Cardinals.N;
            scene.NorthBorders[0] = northBorder;

            Border southBorder = new Border();
            southBorder.BorderLine = new Vector3(0, (int)Constants.RegionSize, 0);    //--->
            southBorder.CrossDirection = Cardinals.S;
            scene.SouthBorders[0] = southBorder;

            Border eastBorder = new Border();
            eastBorder.BorderLine = new Vector3(0, (int)Constants.RegionSize, (int)Constants.RegionSize);   //<---
            eastBorder.CrossDirection = Cardinals.E;
            scene.EastBorders[0] = eastBorder;

            Border westBorder = new Border();
            westBorder.BorderLine = new Vector3(0, (int)Constants.RegionSize, 0);     //--->
            westBorder.CrossDirection = Cardinals.W;
            scene.WestBorders[0] = westBorder;

            RegionConnections regionConnections = new RegionConnections();
            regionConnections.ConnectedRegions = new List<RegionData>();
            regionConnections.RegionScene = scene;
            regionConnections.RegionLandChannel = scene.LandChannel;
            regionConnections.RegionId = scene.RegionInfo.originRegionID;
            regionConnections.X = scene.RegionInfo.RegionLocX;
            regionConnections.Y = scene.RegionInfo.RegionLocY;
            regionConnections.XEnd = (int)Constants.RegionSize;
            regionConnections.YEnd = (int)Constants.RegionSize;
            lock (m_regions)
            {
                bool connectedYN = false;

                foreach (RegionConnections conn in m_regions.Values)
                {
                    #region commented
                    /*
                    // If we're one region over +x +y
                    //xxy
                    //xxx
                    //xxx
                    if ((((int)conn.X * (int)Constants.RegionSize) + conn.XEnd 
                        == (regionConnections.X * (int)Constants.RegionSize)) 
                        && (((int)conn.Y * (int)Constants.RegionSize) - conn.YEnd 
                        == (regionConnections.Y * (int)Constants.RegionSize)))
                    {
                        Vector3 offset = Vector3.Zero;
                        offset.X = (((regionConnections.X * (int) Constants.RegionSize)) -
                                    ((conn.X * (int) Constants.RegionSize)));
                        offset.Y = (((regionConnections.Y * (int) Constants.RegionSize)) -
                                    ((conn.Y * (int) Constants.RegionSize)));

                        Vector3 extents = Vector3.Zero;
                        extents.Y = regionConnections.YEnd + conn.YEnd;
                        extents.X = conn.XEnd + conn.XEnd;

                        m_log.DebugFormat("Scene: {0} to the northwest of Scene{1}.  Offset: {2}.  Extents:{3}",
                                          conn.RegionScene.RegionInfo.RegionName,
                                          regionConnections.RegionScene.RegionInfo.RegionName,
                                          offset, extents);

                        scene.PhysicsScene.Combine(conn.RegionScene.PhysicsScene, offset, extents);
                            
                        connectedYN = true;
                        break;
                    }
                    */

                    /*
                    //If we're one region over x +y
                    //xxx
                    //xxx
                    //xyx
                    if ((((int)conn.X * (int)Constants.RegionSize)
                        == (regionConnections.X * (int)Constants.RegionSize))
                        && (((int)conn.Y * (int)Constants.RegionSize) - conn.YEnd
                        == (regionConnections.Y * (int)Constants.RegionSize)))
                    {
                        Vector3 offset = Vector3.Zero;
                        offset.X = (((regionConnections.X * (int)Constants.RegionSize)) -
                                    ((conn.X * (int)Constants.RegionSize)));
                        offset.Y = (((regionConnections.Y * (int)Constants.RegionSize)) -
                                    ((conn.Y * (int)Constants.RegionSize)));

                        Vector3 extents = Vector3.Zero;
                        extents.Y = regionConnections.YEnd + conn.YEnd;
                        extents.X = conn.XEnd;

                        m_log.DebugFormat("Scene: {0} to the north of Scene{1}.  Offset: {2}. Extents:{3}",
                                          conn.RegionScene.RegionInfo.RegionName,
                                          regionConnections.RegionScene.RegionInfo.RegionName, offset, extents);

                        scene.PhysicsScene.Combine(conn.RegionScene.PhysicsScene, offset, extents);
                        connectedYN = true;
                        break;
                    }
                    */

                    /*
                    // If we're one region over -x +y
                    //xxx
                    //xxx
                    //yxx
                    if ((((int)conn.X * (int)Constants.RegionSize) - conn.XEnd
                        == (regionConnections.X * (int)Constants.RegionSize))
                        && (((int)conn.Y * (int)Constants.RegionSize) - conn.YEnd
                        == (regionConnections.Y * (int)Constants.RegionSize)))
                    {
                        Vector3 offset = Vector3.Zero;
                        offset.X = (((regionConnections.X * (int)Constants.RegionSize)) -
                                    ((conn.X * (int)Constants.RegionSize)));
                        offset.Y = (((regionConnections.Y * (int)Constants.RegionSize)) -
                                    ((conn.Y * (int)Constants.RegionSize)));

                        Vector3 extents = Vector3.Zero;
                        extents.Y = regionConnections.YEnd + conn.YEnd;
                        extents.X = conn.XEnd + conn.XEnd;

                        m_log.DebugFormat("Scene: {0} to the northeast of Scene.  Offset: {2}. Extents:{3}",
                                          conn.RegionScene.RegionInfo.RegionName,
                                          regionConnections.RegionScene.RegionInfo.RegionName, offset, extents);

                        scene.PhysicsScene.Combine(conn.RegionScene.PhysicsScene, offset, extents);


                        connectedYN = true;
                        break;
                    }
                    */

                    /*
                    // If we're one region over -x y
                    //xxx
                    //yxx
                    //xxx
                    if ((((int)conn.X * (int)Constants.RegionSize) - conn.XEnd
                        == (regionConnections.X * (int)Constants.RegionSize))
                        && (((int)conn.Y * (int)Constants.RegionSize)
                        == (regionConnections.Y * (int)Constants.RegionSize)))
                    {
                        Vector3 offset = Vector3.Zero;
                        offset.X = (((regionConnections.X * (int)Constants.RegionSize)) -
                                    ((conn.X * (int)Constants.RegionSize)));
                        offset.Y = (((regionConnections.Y * (int)Constants.RegionSize)) -
                                    ((conn.Y * (int)Constants.RegionSize)));

                        Vector3 extents = Vector3.Zero;
                        extents.Y = regionConnections.YEnd;
                        extents.X = conn.XEnd + conn.XEnd;

                        m_log.DebugFormat("Scene: {0} to the east of Scene{1} Offset: {2}. Extents:{3}",
                                          conn.RegionScene.RegionInfo.RegionName,
                                          regionConnections.RegionScene.RegionInfo.RegionName, offset, extents);

                        scene.PhysicsScene.Combine(conn.RegionScene.PhysicsScene, offset, extents);

                        connectedYN = true;
                        break;
                    }
                    */

                    /*
                        // If we're one region over -x -y
                        //yxx
                        //xxx
                        //xxx
                        if ((((int)conn.X * (int)Constants.RegionSize) - conn.XEnd
                            == (regionConnections.X * (int)Constants.RegionSize))
                            && (((int)conn.Y * (int)Constants.RegionSize) + conn.YEnd
                            == (regionConnections.Y * (int)Constants.RegionSize)))
                        {
                            Vector3 offset = Vector3.Zero;
                            offset.X = (((regionConnections.X * (int)Constants.RegionSize)) -
                                        ((conn.X * (int)Constants.RegionSize)));
                            offset.Y = (((regionConnections.Y * (int)Constants.RegionSize)) -
                                        ((conn.Y * (int)Constants.RegionSize)));

                            Vector3 extents = Vector3.Zero;
                            extents.Y = regionConnections.YEnd + conn.YEnd;
                            extents.X = conn.XEnd + conn.XEnd;

                            m_log.DebugFormat("Scene: {0} to the northeast of Scene{1} Offset: {2}. Extents:{3}",
                                              conn.RegionScene.RegionInfo.RegionName,
                                              regionConnections.RegionScene.RegionInfo.RegionName, offset, extents);

                            scene.PhysicsScene.Combine(conn.RegionScene.PhysicsScene, offset, extents);

                            connectedYN = true;
                            break;
                        }
                        */
                    #endregion

                    // If we're one region over +x y
                    //xxx
                    //xxy
                    //xxx
                    if ((((int)conn.X * (int)Constants.RegionSize) + conn.XEnd
                        >= (regionConnections.X * (int)Constants.RegionSize))
                        && (((int)conn.Y * (int)Constants.RegionSize)
                        >= (regionConnections.Y * (int)Constants.RegionSize)))
                    {
                        Vector3 offset = Vector3.Zero;
                        offset.X = (((regionConnections.X * (int)Constants.RegionSize)) -
                                    ((conn.X * (int)Constants.RegionSize)));
                        offset.Y = (((regionConnections.Y * (int)Constants.RegionSize)) -
                                    ((conn.Y * (int)Constants.RegionSize)));

                        Vector3 extents = Vector3.Zero;
                        extents.Y = conn.YEnd;
                        extents.X = conn.XEnd + regionConnections.XEnd;

                        conn.UpdateExtents(extents);


                        m_log.DebugFormat("Scene: {0} to the west of Scene{1} Offset: {2}. Extents:{3}",
                                          conn.RegionScene.RegionInfo.RegionName,
                                          regionConnections.RegionScene.RegionInfo.RegionName, offset, extents);
                        
                        
                        scene.BordersLocked = true;
                        conn.RegionScene.BordersLocked = true;

                        RegionData ConnectedRegion = new RegionData();
                        ConnectedRegion.Offset = offset;
                        ConnectedRegion.RegionId = scene.RegionInfo.originRegionID;
                        ConnectedRegion.RegionScene = scene;
                        conn.ConnectedRegions.Add(ConnectedRegion);

                        conn.RegionScene.PhysicsScene.Combine(null, Vector3.Zero, extents);
                        scene.PhysicsScene.Combine(conn.RegionScene.PhysicsScene, offset, Vector3.Zero);

                        lock (conn.RegionScene.EastBorders)
                            conn.RegionScene.EastBorders[0].BorderLine.Z += (int)Constants.RegionSize;

                        lock (conn.RegionScene.NorthBorders)
                            conn.RegionScene.NorthBorders[0].BorderLine.Y += (int)Constants.RegionSize;

                        lock (conn.RegionScene.SouthBorders)
                            conn.RegionScene.SouthBorders[0].BorderLine.Y += (int)Constants.RegionSize;

                        lock (scene.WestBorders)
                            scene.WestBorders[0].BorderLine.Z += (int)Constants.RegionSize; //auto teleport West

                        // Reset Terrain..  since terrain normally loads first.
                        //
                        scene.PhysicsScene.SetTerrain(scene.Heightmap.GetFloatsSerialised());
                        //conn.RegionScene.PhysicsScene.SetTerrain(conn.RegionScene.Heightmap.GetFloatsSerialised());
                        
                        conn.RegionScene.BordersLocked = false;
                        scene.BordersLocked = false;
                        connectedYN = true;
                        break;
                    }



                    // If we're one region over x +y
                    //xyx
                    //xxx
                    //xxx
                    if ((((int)conn.X * (int)Constants.RegionSize)
                        >= (regionConnections.X * (int)Constants.RegionSize))
                        && (((int)conn.Y * (int)Constants.RegionSize) + conn.YEnd
                        >= (regionConnections.Y * (int)Constants.RegionSize)))
                    {
                        Vector3 offset = Vector3.Zero;
                        offset.X = (((regionConnections.X * (int)Constants.RegionSize)) -
                                    ((conn.X * (int)Constants.RegionSize)));
                        offset.Y = (((regionConnections.Y * (int)Constants.RegionSize)) -
                                    ((conn.Y * (int)Constants.RegionSize)));

                        Vector3 extents = Vector3.Zero;
                        extents.Y = regionConnections.YEnd + conn.YEnd;
                        extents.X = conn.XEnd;
                        conn.UpdateExtents(extents);


                        scene.BordersLocked = true;
                        conn.RegionScene.BordersLocked = true;

                        RegionData ConnectedRegion = new RegionData();
                        ConnectedRegion.Offset = offset;
                        ConnectedRegion.RegionId = scene.RegionInfo.originRegionID;
                        ConnectedRegion.RegionScene = scene;
                        conn.ConnectedRegions.Add(ConnectedRegion);

                        m_log.DebugFormat("Scene: {0} to the northeast of Scene{1} Offset: {2}. Extents:{3}",
                                         conn.RegionScene.RegionInfo.RegionName,
                                         regionConnections.RegionScene.RegionInfo.RegionName, offset, extents);
                        conn.RegionScene.PhysicsScene.Combine(null,Vector3.Zero,extents);
                        scene.PhysicsScene.Combine(conn.RegionScene.PhysicsScene, offset, Vector3.Zero);

                        lock (conn.RegionScene.NorthBorders)
                            conn.RegionScene.NorthBorders[0].BorderLine.Z += (int)Constants.RegionSize;
                        lock (conn.RegionScene.EastBorders)
                            conn.RegionScene.EastBorders[0].BorderLine.Y += (int)Constants.RegionSize;
                        lock (conn.RegionScene.WestBorders)
                            conn.RegionScene.WestBorders[0].BorderLine.Y += (int)Constants.RegionSize;
                        lock (scene.SouthBorders)
                            scene.SouthBorders[0].BorderLine.Z += (int)Constants.RegionSize; //auto teleport south

                        // Reset Terrain..  since terrain normally loads first.
                        //conn.RegionScene.PhysicsScene.SetTerrain(conn.RegionScene.Heightmap.GetFloatsSerialised());
                        scene.PhysicsScene.SetTerrain(scene.Heightmap.GetFloatsSerialised());
                        //conn.RegionScene.PhysicsScene.SetTerrain(conn.RegionScene.Heightmap.GetFloatsSerialised());

                        scene.BordersLocked = false;
                        conn.RegionScene.BordersLocked = false;

                        connectedYN = true;
                        break;
                    }

                    // If we're one region over +x +y
                    //xxy
                    //xxx
                    //xxx
                    if ((((int)conn.X * (int)Constants.RegionSize) + conn.YEnd
                        >= (regionConnections.X * (int)Constants.RegionSize))
                        && (((int)conn.Y * (int)Constants.RegionSize) + conn.YEnd
                        >= (regionConnections.Y * (int)Constants.RegionSize)))
                    {
                        Vector3 offset = Vector3.Zero;
                        offset.X = (((regionConnections.X * (int)Constants.RegionSize)) -
                                    ((conn.X * (int)Constants.RegionSize)));
                        offset.Y = (((regionConnections.Y * (int)Constants.RegionSize)) -
                                    ((conn.Y * (int)Constants.RegionSize)));

                        Vector3 extents = Vector3.Zero;
                        extents.Y = regionConnections.YEnd + conn.YEnd;
                        extents.X = regionConnections.XEnd + conn.XEnd;
                        conn.UpdateExtents(extents);

                        scene.BordersLocked = true;
                        conn.RegionScene.BordersLocked = true;

                        RegionData ConnectedRegion = new RegionData();
                        ConnectedRegion.Offset = offset;
                        ConnectedRegion.RegionId = scene.RegionInfo.originRegionID;
                        ConnectedRegion.RegionScene = scene;

                        conn.ConnectedRegions.Add(ConnectedRegion);

                        m_log.DebugFormat("Scene: {0} to the NorthEast of Scene{1} Offset: {2}. Extents:{3}",
                                         conn.RegionScene.RegionInfo.RegionName,
                                         regionConnections.RegionScene.RegionInfo.RegionName, offset, extents);
                        
                        conn.RegionScene.PhysicsScene.Combine(null, Vector3.Zero, extents);
                        scene.PhysicsScene.Combine(conn.RegionScene.PhysicsScene, offset, Vector3.Zero);
                        lock (conn.RegionScene.NorthBorders)
                        if (conn.RegionScene.NorthBorders.Count == 1)// &&  2)
                        {
                            //compound border
                            // already locked above
                            conn.RegionScene.NorthBorders[0].BorderLine.Z += (int)Constants.RegionSize;

                            lock (conn.RegionScene.EastBorders)
                                conn.RegionScene.EastBorders[0].BorderLine.Y += (int)Constants.RegionSize;
                            lock (conn.RegionScene.WestBorders)
                                conn.RegionScene.WestBorders[0].BorderLine.Y += (int)Constants.RegionSize;

                            

                        }
                        lock (scene.SouthBorders)
                            scene.SouthBorders[0].BorderLine.Z += (int)Constants.RegionSize; //auto teleport south
                        
                        lock (conn.RegionScene.EastBorders)
                        if (conn.RegionScene.EastBorders.Count == 1)// && conn.RegionScene.EastBorders.Count == 2)
                        {

                            conn.RegionScene.EastBorders[0].BorderLine.Z += (int)Constants.RegionSize;
                            lock (conn.RegionScene.NorthBorders)
                                conn.RegionScene.NorthBorders[0].BorderLine.Y += (int)Constants.RegionSize;
                            lock (conn.RegionScene.SouthBorders)
                                conn.RegionScene.SouthBorders[0].BorderLine.Y += (int)Constants.RegionSize;

                            
                        }

                        lock (scene.WestBorders)
                            scene.WestBorders[0].BorderLine.Z += (int)Constants.RegionSize; //auto teleport West
/*  
                        else
                        {
                            conn.RegionScene.NorthBorders[0].BorderLine.Z += (int)Constants.RegionSize;
                            conn.RegionScene.EastBorders[0].BorderLine.Y += (int)Constants.RegionSize;
                            conn.RegionScene.WestBorders[0].BorderLine.Y += (int)Constants.RegionSize;
                            scene.SouthBorders[0].BorderLine.Z += (int)Constants.RegionSize; //auto teleport south
                        }
*/
                        

                        // Reset Terrain..  since terrain normally loads first.
                        //conn.RegionScene.PhysicsScene.SetTerrain(conn.RegionScene.Heightmap.GetFloatsSerialised());
                        scene.PhysicsScene.SetTerrain(scene.Heightmap.GetFloatsSerialised());
                        //conn.RegionScene.PhysicsScene.SetTerrain(conn.RegionScene.Heightmap.GetFloatsSerialised());
                        scene.BordersLocked = false;
                        conn.RegionScene.BordersLocked = false;

                        connectedYN = true;

                        //scene.PhysicsScene.Combine(conn.RegionScene.PhysicsScene, offset,extents);

                        break;
                    }


                }
                if (!connectedYN)
                {
                    RegionData rdata = new RegionData();
                    rdata.Offset = Vector3.Zero;
                    rdata.RegionId = scene.RegionInfo.originRegionID;
                    rdata.RegionScene = scene;
                    regionConnections.RegionLandChannel = scene.LandChannel;

                    RegionCombinerLargeLandChannel lnd = new RegionCombinerLargeLandChannel(rdata,scene.LandChannel,regionConnections.ConnectedRegions);
                    scene.LandChannel = lnd;
                    lock (m_regions)
                    {
                        foreach (RegionData r in regionConnections.ConnectedRegions)
                        {
                            ForwardPermissionRequests(regionConnections, r.RegionScene);
                        }
                    }
                    
                    m_regions.Add(scene.RegionInfo.originRegionID,regionConnections);
                }
                    
            }
            AdjustLargeRegionBounds();
            
        }

        public void RemoveRegion(Scene scene)
        {
           
                
        }

        public void RegionLoaded(Scene scene)
        {
            
        }

        public void PostInitialise()
        {
            
        }
        
        public void UnCombineRegion(RegionData rdata)
        {
            lock (m_regions)
            {
                if (m_regions.ContainsKey(rdata.RegionId))
                {
                    // uncombine root region and virtual regions
                }
                else
                {
                    foreach (RegionConnections r in m_regions.Values)
                    {
                        foreach (RegionData rd in r.ConnectedRegions)
                        {
                            if (rd.RegionId == rdata.RegionId)
                            {
                                // uncombine virtual region
                            }
                        }
                    }
                }
            }
        }
        // Create a set of infinite borders around the whole aabb of the combined island.
        private void AdjustLargeRegionBounds()
        {
            lock (m_regions)
            {
                foreach (RegionConnections rconn in m_regions.Values)
                {
                    Vector3 offset = Vector3.Zero;
                    rconn.RegionScene.BordersLocked = true;
                    foreach (RegionData rdata in rconn.ConnectedRegions)
                    {
                        if (rdata.Offset.X > offset.X) offset.X = rdata.Offset.X;
                        if (rdata.Offset.Y > offset.Y) offset.Y = rdata.Offset.Y;

                    }

                    lock (rconn.RegionScene.NorthBorders)
                    {
                        
                        Border northBorder = null;
                        
                        if (!TryGetInfiniteBorder(rconn.RegionScene.NorthBorders, out northBorder))
                        {
                            northBorder = new Border();
                            rconn.RegionScene.NorthBorders.Add(northBorder);
                        }
                        
                        northBorder.BorderLine = new Vector3(float.MinValue, float.MaxValue,
                                                             offset.Y + (int) Constants.RegionSize); //<---
                        northBorder.CrossDirection = Cardinals.N;
                        
                    }

                    lock (rconn.RegionScene.SouthBorders)
                    {
                        Border southBorder = null;
                        if (!TryGetInfiniteBorder(rconn.RegionScene.SouthBorders, out southBorder))
                        {
                            southBorder = new Border();
                            rconn.RegionScene.SouthBorders.Add(southBorder);
                        }
                        southBorder.BorderLine = new Vector3(float.MinValue, float.MaxValue, 0); //--->
                        southBorder.CrossDirection = Cardinals.S;
                        
                    }

                    lock (rconn.RegionScene.EastBorders)
                    {
                        Border eastBorder = null;
                        if (!TryGetInfiniteBorder(rconn.RegionScene.EastBorders, out eastBorder))
                        {
                            eastBorder = new Border();
                            rconn.RegionScene.EastBorders.Add(eastBorder);
                        }
                        eastBorder.BorderLine = new Vector3(float.MinValue, float.MaxValue, offset.X + (int)Constants.RegionSize);
                        //<---
                        eastBorder.CrossDirection = Cardinals.E;
                        
                    }

                    lock (rconn.RegionScene.WestBorders)
                    {
                        Border westBorder = null;
                        if (!TryGetInfiniteBorder(rconn.RegionScene.WestBorders, out westBorder))
                        {
                            westBorder = new Border();
                            rconn.RegionScene.WestBorders.Add(westBorder);

                        }
                        westBorder.BorderLine = new Vector3(float.MinValue, float.MaxValue, 0); //--->
                        westBorder.CrossDirection = Cardinals.W;
                        
                    }



                    rconn.RegionScene.BordersLocked = false;
                }
            }
        }

        public static bool TryGetInfiniteBorder(List<Border> borders, out Border oborder)
        {
            // Warning! Should be locked before getting here!
            foreach (Border b in borders)
            {
                if (b.BorderLine.X == float.MinValue && b.BorderLine.Y == float.MaxValue)
                {
                    oborder = b;
                    return true;
                }
            }
            oborder = null;
            return false;
        }
       
        public RegionData GetRegionFromPosition(Vector3 pPosition)
        {
            pPosition = pPosition/(int) Constants.RegionSize;
            int OffsetX = (int) pPosition.X;
            int OffsetY = (int) pPosition.Y;
            foreach (RegionConnections regConn in m_regions.Values)
            {
                foreach (RegionData reg in regConn.ConnectedRegions)
                {
                    if (reg.Offset.X == OffsetX && reg.Offset.Y == OffsetY)
                        return reg;
                }
            }
            return new RegionData();
        }

        public void ForwardPermissionRequests(RegionConnections BigRegion, Scene VirtualRegion)
        {
            if (BigRegion.PermissionModule == null)
                BigRegion.PermissionModule = new RegionCombinerPermissionModule(BigRegion.RegionScene);



            VirtualRegion.Permissions.OnBypassPermissions += BigRegion.PermissionModule.BypassPermissions;
            VirtualRegion.Permissions.OnSetBypassPermissions += BigRegion.PermissionModule.SetBypassPermissions;
            VirtualRegion.Permissions.OnPropagatePermissions += BigRegion.PermissionModule.PropagatePermissions;
            VirtualRegion.Permissions.OnGenerateClientFlags += BigRegion.PermissionModule.GenerateClientFlags;
            VirtualRegion.Permissions.OnAbandonParcel += BigRegion.PermissionModule.CanAbandonParcel;
            VirtualRegion.Permissions.OnReclaimParcel += BigRegion.PermissionModule.CanReclaimParcel;
            VirtualRegion.Permissions.OnDeedParcel += BigRegion.PermissionModule.CanDeedParcel;
            VirtualRegion.Permissions.OnDeedObject += BigRegion.PermissionModule.CanDeedObject;
            VirtualRegion.Permissions.OnIsGod += BigRegion.PermissionModule.IsGod;
            VirtualRegion.Permissions.OnDuplicateObject += BigRegion.PermissionModule.CanDuplicateObject;
            VirtualRegion.Permissions.OnDeleteObject += BigRegion.PermissionModule.CanDeleteObject; //MAYBE FULLY IMPLEMENTED
            VirtualRegion.Permissions.OnEditObject += BigRegion.PermissionModule.CanEditObject; //MAYBE FULLY IMPLEMENTED
            VirtualRegion.Permissions.OnEditParcel += BigRegion.PermissionModule.CanEditParcel; //MAYBE FULLY IMPLEMENTED            
            VirtualRegion.Permissions.OnInstantMessage += BigRegion.PermissionModule.CanInstantMessage;
            VirtualRegion.Permissions.OnInventoryTransfer += BigRegion.PermissionModule.CanInventoryTransfer; //NOT YET IMPLEMENTED
            VirtualRegion.Permissions.OnIssueEstateCommand += BigRegion.PermissionModule.CanIssueEstateCommand; //FULLY IMPLEMENTED
            VirtualRegion.Permissions.OnMoveObject += BigRegion.PermissionModule.CanMoveObject; //MAYBE FULLY IMPLEMENTED
            VirtualRegion.Permissions.OnObjectEntry += BigRegion.PermissionModule.CanObjectEntry;
            VirtualRegion.Permissions.OnReturnObject += BigRegion.PermissionModule.CanReturnObject; //NOT YET IMPLEMENTED
            VirtualRegion.Permissions.OnRezObject += BigRegion.PermissionModule.CanRezObject; //MAYBE FULLY IMPLEMENTED
            VirtualRegion.Permissions.OnRunConsoleCommand += BigRegion.PermissionModule.CanRunConsoleCommand;
            VirtualRegion.Permissions.OnRunScript += BigRegion.PermissionModule.CanRunScript; //NOT YET IMPLEMENTED
            VirtualRegion.Permissions.OnCompileScript += BigRegion.PermissionModule.CanCompileScript;
            VirtualRegion.Permissions.OnSellParcel += BigRegion.PermissionModule.CanSellParcel;
            VirtualRegion.Permissions.OnTakeObject += BigRegion.PermissionModule.CanTakeObject;
            VirtualRegion.Permissions.OnTakeCopyObject += BigRegion.PermissionModule.CanTakeCopyObject;
            VirtualRegion.Permissions.OnTerraformLand += BigRegion.PermissionModule.CanTerraformLand;
            VirtualRegion.Permissions.OnLinkObject += BigRegion.PermissionModule.CanLinkObject; //NOT YET IMPLEMENTED
            VirtualRegion.Permissions.OnDelinkObject += BigRegion.PermissionModule.CanDelinkObject; //NOT YET IMPLEMENTED
            VirtualRegion.Permissions.OnBuyLand += BigRegion.PermissionModule.CanBuyLand; //NOT YET IMPLEMENTED

            VirtualRegion.Permissions.OnViewNotecard += BigRegion.PermissionModule.CanViewNotecard; //NOT YET IMPLEMENTED
            VirtualRegion.Permissions.OnViewScript += BigRegion.PermissionModule.CanViewScript; //NOT YET IMPLEMENTED                       
            VirtualRegion.Permissions.OnEditNotecard += BigRegion.PermissionModule.CanEditNotecard; //NOT YET IMPLEMENTED            
            VirtualRegion.Permissions.OnEditScript += BigRegion.PermissionModule.CanEditScript; //NOT YET IMPLEMENTED            

            VirtualRegion.Permissions.OnCreateObjectInventory += BigRegion.PermissionModule.CanCreateObjectInventory; //NOT IMPLEMENTED HERE 
            VirtualRegion.Permissions.OnEditObjectInventory += BigRegion.PermissionModule.CanEditObjectInventory;//MAYBE FULLY IMPLEMENTED            
            VirtualRegion.Permissions.OnCopyObjectInventory += BigRegion.PermissionModule.CanCopyObjectInventory; //NOT YET IMPLEMENTED
            VirtualRegion.Permissions.OnDeleteObjectInventory += BigRegion.PermissionModule.CanDeleteObjectInventory; //NOT YET IMPLEMENTED
            VirtualRegion.Permissions.OnResetScript += BigRegion.PermissionModule.CanResetScript;

            VirtualRegion.Permissions.OnCreateUserInventory += BigRegion.PermissionModule.CanCreateUserInventory; //NOT YET IMPLEMENTED
            VirtualRegion.Permissions.OnCopyUserInventory += BigRegion.PermissionModule.CanCopyUserInventory; //NOT YET IMPLEMENTED
            VirtualRegion.Permissions.OnEditUserInventory += BigRegion.PermissionModule.CanEditUserInventory; //NOT YET IMPLEMENTED
            VirtualRegion.Permissions.OnDeleteUserInventory += BigRegion.PermissionModule.CanDeleteUserInventory; //NOT YET IMPLEMENTED

            VirtualRegion.Permissions.OnTeleport += BigRegion.PermissionModule.CanTeleport; //NOT YET IMPLEMENTED
            VirtualRegion.Permissions.OnUseObjectReturn += BigRegion.PermissionModule.CanUseObjectReturn; //NOT YET IMPLEMENTED
        }
    }
    public class RegionConnections
    {
        public UUID RegionId;
        public Scene RegionScene;
        public ILandChannel RegionLandChannel;
        public uint X;
        public uint Y;
        public int XEnd;
        public int YEnd;
        public List<RegionData> ConnectedRegions;
        public RegionCombinerPermissionModule PermissionModule;
        public void UpdateExtents(Vector3 extents)
        {
            XEnd = (int)extents.X;
            YEnd = (int)extents.Y;
        }

    }

    public class RegionData
    {
        public UUID RegionId;
        public Scene RegionScene;
        public Vector3 Offset;
        
    }

    public class RegionCombinerLargeLandChannel : ILandChannel
    {
        // private static readonly ILog m_log =
        //     LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private RegionData RegData;
        private ILandChannel RootRegionLandChannel;
        private readonly List<RegionData> RegionConnections;
        
        #region ILandChannel Members

        public RegionCombinerLargeLandChannel(RegionData regData, ILandChannel rootRegionLandChannel,List<RegionData> regionConnections)
        {
            RegData = regData;
            RootRegionLandChannel = rootRegionLandChannel;
            RegionConnections = regionConnections;
        }

        public List<ILandObject> ParcelsNearPoint(Vector3 position)
        {
            //m_log.DebugFormat("[LANDPARCELNEARPOINT]: {0}>", position);
            return RootRegionLandChannel.ParcelsNearPoint(position - RegData.Offset);
        }

        public List<ILandObject> AllParcels()
        {

            return RootRegionLandChannel.AllParcels();

        }

        public ILandObject GetLandObject(int x, int y)
        {
            //m_log.DebugFormat("[BIGLANDTESTINT]: <{0},{1}>", x, y);

            if (x > 0 && x <= (int)Constants.RegionSize && y > 0 && y <= (int)Constants.RegionSize)
            {
                return RootRegionLandChannel.GetLandObject(x, y);
            }
            else
            {
                int offsetX = (x / (int)Constants.RegionSize);
                int offsetY = (x / (int)Constants.RegionSize);
                offsetX *= (int)Constants.RegionSize;
                offsetY *= (int)Constants.RegionSize;

                foreach (RegionData regionData in RegionConnections)
                {
                    if (regionData.Offset.X == offsetX && regionData.Offset.Y == offsetY)
                    {
                        return regionData.RegionScene.LandChannel.GetLandObject(x - offsetX, y - offsetY);
                    }
                }
                ILandObject obj = new LandObject(UUID.Zero, false, RegData.RegionScene);
                obj.landData.Name = "NO LAND";
                return obj;
            }
        }

        public ILandObject GetLandObject(int localID)
        {
            return RootRegionLandChannel.GetLandObject(localID);
        }

        public ILandObject GetLandObject(float x, float y)
        {
            //m_log.DebugFormat("[BIGLANDTESTFLOAT]: <{0},{1}>", x, y);
            
            if (x > 0 && x <= (int)Constants.RegionSize && y > 0 && y <= (int)Constants.RegionSize)
            {
                return RootRegionLandChannel.GetLandObject(x, y);
            }
            else
            {
                int offsetX = (int)(x/(int) Constants.RegionSize);
                int offsetY = (int)(x/(int) Constants.RegionSize);
                offsetX *= (int) Constants.RegionSize;
                offsetY *= (int) Constants.RegionSize;

                foreach (RegionData regionData in RegionConnections)
                {
                    if (regionData.Offset.X == offsetX && regionData.Offset.Y == offsetY)
                    {
                        return regionData.RegionScene.LandChannel.GetLandObject(x - offsetX, y - offsetY);
                    }
                }
                ILandObject obj = new LandObject(UUID.Zero, false, RegData.RegionScene);
                obj.landData.Name = "NO LAND";
                return obj;
            }
        }

        public bool IsLandPrimCountTainted()
        {
            return RootRegionLandChannel.IsLandPrimCountTainted();
        }

        public bool IsForcefulBansAllowed()
        {
            return RootRegionLandChannel.IsForcefulBansAllowed();
        }

        public void UpdateLandObject(int localID, LandData data)
        {
            RootRegionLandChannel.UpdateLandObject(localID, data);
        }

        public void ReturnObjectsInParcel(int localID, uint returnType, UUID[] agentIDs, UUID[] taskIDs, IClientAPI remoteClient)
        {
            RootRegionLandChannel.ReturnObjectsInParcel(localID, returnType, agentIDs, taskIDs, remoteClient);
        }

        public void setParcelObjectMaxOverride(overrideParcelMaxPrimCountDelegate overrideDel)
        {
            RootRegionLandChannel.setParcelObjectMaxOverride(overrideDel);
        }

        public void setSimulatorObjectMaxOverride(overrideSimulatorMaxPrimCountDelegate overrideDel)
        {
            RootRegionLandChannel.setSimulatorObjectMaxOverride(overrideDel);
        }

        public void SetParcelOtherCleanTime(IClientAPI remoteClient, int localID, int otherCleanTime)
        {
            RootRegionLandChannel.SetParcelOtherCleanTime(remoteClient, localID, otherCleanTime);
        }

        #endregion
    }

    public class RegionCombinerPermissionModule
    {
        private Scene m_rootScene;
        public RegionCombinerPermissionModule(Scene RootScene)
        {
            m_rootScene = RootScene;
        }

        public bool BypassPermissions()
        {
            return m_rootScene.Permissions.BypassPermissions();
        }

        public void SetBypassPermissions(bool value)
        {
            m_rootScene.Permissions.SetBypassPermissions(value);
        }

        public bool PropagatePermissions()
        {
            return m_rootScene.Permissions.PropagatePermissions();
        }

        public uint GenerateClientFlags(UUID userid, UUID objectidid)
        {
            return m_rootScene.Permissions.GenerateClientFlags(userid,objectidid);
        }

        public bool CanAbandonParcel(UUID user, ILandObject parcel, Scene scene)
        {
            return m_rootScene.Permissions.CanAbandonParcel(user,parcel);
        }

        public bool CanReclaimParcel(UUID user, ILandObject parcel, Scene scene)
        {
            return m_rootScene.Permissions.CanReclaimParcel(user, parcel);
        }

        public bool CanDeedParcel(UUID user, ILandObject parcel, Scene scene)
        {
            return m_rootScene.Permissions.CanDeedParcel(user, parcel);
        }

        public bool CanDeedObject(UUID user, UUID @group, Scene scene)
        {
            return m_rootScene.Permissions.CanDeedObject(user,@group);
        }

        public bool IsGod(UUID user, Scene requestfromscene)
        {
            return m_rootScene.Permissions.IsGod(user);
        }

        public bool CanDuplicateObject(int objectcount, UUID objectid, UUID owner, Scene scene, Vector3 objectposition)
        {
            return m_rootScene.Permissions.CanDuplicateObject(objectcount, objectid, owner, objectposition);
        }

        public bool CanDeleteObject(UUID objectid, UUID deleter, Scene scene)
        {
            return m_rootScene.Permissions.CanDeleteObject(objectid, deleter);
        }

        public bool CanEditObject(UUID objectid, UUID editorid, Scene scene)
        {
            return m_rootScene.Permissions.CanEditObject(objectid, editorid);
        }

        public bool CanEditParcel(UUID user, ILandObject parcel, Scene scene)
        {
            return m_rootScene.Permissions.CanEditParcel(user, parcel);
        }

        public bool CanInstantMessage(UUID user, UUID target, Scene startscene)
        {
            return m_rootScene.Permissions.CanInstantMessage(user, target);
        }

        public bool CanInventoryTransfer(UUID user, UUID target, Scene startscene)
        {
            return m_rootScene.Permissions.CanInventoryTransfer(user, target);
        }

        public bool CanIssueEstateCommand(UUID user, Scene requestfromscene, bool ownercommand)
        {
            return m_rootScene.Permissions.CanIssueEstateCommand(user, ownercommand);
        }

        public bool CanMoveObject(UUID objectid, UUID moverid, Scene scene)
        {
            return m_rootScene.Permissions.CanMoveObject(objectid, moverid);
        }

        public bool CanObjectEntry(UUID objectid, bool enteringregion, Vector3 newpoint, Scene scene)
        {
            return m_rootScene.Permissions.CanObjectEntry(objectid, enteringregion, newpoint);
        }

        public bool CanReturnObject(UUID objectid, UUID returnerid, Scene scene)
        {
            return m_rootScene.Permissions.CanReturnObject(objectid, returnerid);
        }

        public bool CanRezObject(int objectcount, UUID owner, Vector3 objectposition, Scene scene)
        {
            return m_rootScene.Permissions.CanRezObject(objectcount, owner, objectposition);
        }

        public bool CanRunConsoleCommand(UUID user, Scene requestfromscene)
        {
            return m_rootScene.Permissions.CanRunConsoleCommand(user);
        }

        public bool CanRunScript(UUID script, UUID objectid, UUID user, Scene scene)
        {
            return m_rootScene.Permissions.CanRunScript(script, objectid, user);
        }

        public bool CanCompileScript(UUID owneruuid, int scripttype, Scene scene)
        {
            return m_rootScene.Permissions.CanCompileScript(owneruuid, scripttype);
        }

        public bool CanSellParcel(UUID user, ILandObject parcel, Scene scene)
        {
            return m_rootScene.Permissions.CanSellParcel(user, parcel);
        }

        public bool CanTakeObject(UUID objectid, UUID stealer, Scene scene)
        {
            return m_rootScene.Permissions.CanTakeObject(objectid, stealer);
        }

        public bool CanTakeCopyObject(UUID objectid, UUID userid, Scene inscene)
        {
            return m_rootScene.Permissions.CanTakeObject(objectid, userid);
        }

        public bool CanTerraformLand(UUID user, Vector3 position, Scene requestfromscene)
        {
            return m_rootScene.Permissions.CanTerraformLand(user, position);
        }

        public bool CanLinkObject(UUID user, UUID objectid)
        {
            return m_rootScene.Permissions.CanLinkObject(user, objectid);
        }

        public bool CanDelinkObject(UUID user, UUID objectid)
        {
            return m_rootScene.Permissions.CanDelinkObject(user, objectid);
        }

        public bool CanBuyLand(UUID user, ILandObject parcel, Scene scene)
        {
            return m_rootScene.Permissions.CanBuyLand(user, parcel);
        }

        public bool CanViewNotecard(UUID script, UUID objectid, UUID user, Scene scene)
        {
            return m_rootScene.Permissions.CanViewNotecard(script, objectid, user);
        }

        public bool CanViewScript(UUID script, UUID objectid, UUID user, Scene scene)
        {
            return m_rootScene.Permissions.CanViewScript(script, objectid, user);
        }

        public bool CanEditNotecard(UUID notecard, UUID objectid, UUID user, Scene scene)
        {
            return m_rootScene.Permissions.CanEditNotecard(notecard, objectid, user);
        }

        public bool CanEditScript(UUID script, UUID objectid, UUID user, Scene scene)
        {
            return m_rootScene.Permissions.CanEditScript(script, objectid, user);
        }

        public bool CanCreateObjectInventory(int invtype, UUID objectid, UUID userid)
        {
            return m_rootScene.Permissions.CanCreateObjectInventory(invtype, objectid, userid);
        }

        public bool CanEditObjectInventory(UUID objectid, UUID editorid, Scene scene)
        {
            return m_rootScene.Permissions.CanEditObjectInventory(objectid, editorid);
        }

        public bool CanCopyObjectInventory(UUID itemid, UUID objectid, UUID userid)
        {
            return m_rootScene.Permissions.CanCopyObjectInventory(itemid, objectid, userid);
        }

        public bool CanDeleteObjectInventory(UUID itemid, UUID objectid, UUID userid)
        {
            return m_rootScene.Permissions.CanDeleteObjectInventory(itemid, objectid, userid);
        }

        public bool CanResetScript(UUID prim, UUID script, UUID user, Scene scene)
        {
            return m_rootScene.Permissions.CanResetScript(prim, script, user);
        }

        public bool CanCreateUserInventory(int invtype, UUID userid)
        {
            return m_rootScene.Permissions.CanCreateUserInventory(invtype, userid);
        }

        public bool CanCopyUserInventory(UUID itemid, UUID userid)
        {
            return m_rootScene.Permissions.CanCopyUserInventory(itemid, userid);
        }

        public bool CanEditUserInventory(UUID itemid, UUID userid)
        {
            return m_rootScene.Permissions.CanEditUserInventory(itemid, userid);
        }

        public bool CanDeleteUserInventory(UUID itemid, UUID userid)
        {
            return m_rootScene.Permissions.CanDeleteUserInventory(itemid, userid);
        }

        public bool CanTeleport(UUID userid, Scene scene)
        {
            return m_rootScene.Permissions.CanTeleport(userid);
        }

        public bool CanUseObjectReturn(ILandObject landdata, uint type, IClientAPI client, List<SceneObjectGroup> retlist, Scene scene)
        {
            return m_rootScene.Permissions.CanUseObjectReturn(landdata, type, client, retlist);
        }
    }
}