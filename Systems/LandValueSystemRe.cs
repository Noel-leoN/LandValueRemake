#define UNITY_ASSERTIONS
using System.Runtime.CompilerServices;
using Colossal.Collections;
//using Colossal.Mathematics;
using Colossal.Serialization.Entities;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using Game.Simulation;
using Unity.Assertions;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting;
using Game;
using System.Security.Cryptography;
using Game.Areas;
using Game.Buildings;
using Game.Citizens;
using Game.Objects;
using Game.Economy;
using System.Runtime.InteropServices;
using System;

namespace LandValueRemake.Systems
{
	public partial class LandValueSystemRe : CellMapSystem<LandValueCell>, IJobSerializable
	{
        private struct NetIterator : INativeQuadTreeIterator<Entity, QuadTreeBoundsXZ>, IUnsafeQuadTreeIterator<Entity, QuadTreeBoundsXZ>
        {
            public int m_TotalCount;

            public float m_TotalLandValueBonus;

            public Colossal.Mathematics.Bounds3 m_Bounds;

            public ComponentLookup<LandValue> m_LandValueData;

            public ComponentLookup<EdgeGeometry> m_EdgeGeometryData;

            public bool Intersect(QuadTreeBoundsXZ bounds)
            {
                return Colossal.Mathematics.MathUtils.Intersect(bounds.m_Bounds, this.m_Bounds);
            }

            public void Iterate(QuadTreeBoundsXZ bounds, Entity entity)
            {
                if (Colossal.Mathematics.MathUtils.Intersect(bounds.m_Bounds, this.m_Bounds) && this.m_LandValueData.HasComponent(entity) && this.m_EdgeGeometryData.HasComponent(entity))
                {
                    LandValue landValue = this.m_LandValueData[entity];
                    if (landValue.m_LandValue > 0f)
                    {
                        this.m_TotalLandValueBonus += landValue.m_LandValue;
                        this.m_TotalCount++;
                    }
                }
            }
        }

        [BurstCompile]
        private struct LandValueMapUpdateJob : IJobParallelFor
        {
            public NativeArray<LandValueCell> m_LandValueMap;

            [ReadOnly]
            public NativeQuadTree<Entity, QuadTreeBoundsXZ> m_NetSearchTree;            

            [ReadOnly]
            public ComponentLookup<LandValue> m_LandValueData;

            [ReadOnly]
            public ComponentLookup<EdgeGeometry> m_EdgeGeometryData;
           
            [ReadOnly]
            public LandValueParameterData m_LandValueParameterData;

            public float m_CellSize;

            public void Execute(int index)
            {
                float3 cellCenter = CellMapSystem<LandValueCell>.GetCellCenter(index, LandValueSystemRe.kTextureSize);
                //if (WaterUtils.SampleDepth(ref this.m_WaterSurfaceData, cellCenter) > 1f)
                //{
                this.m_LandValueMap[index] = new LandValueCell
                {
                    m_LandValue = this.m_LandValueParameterData.m_LandValueBaseline
                };
                //return;
                //}
                NetIterator netIterator = default(NetIterator);
                netIterator.m_TotalCount = 0;
                netIterator.m_TotalLandValueBonus = 0f;
                netIterator.m_Bounds = new Colossal.Mathematics.Bounds3(cellCenter - new float3(1.5f * this.m_CellSize, 10000f, 1.5f * this.m_CellSize), cellCenter + new float3(1.5f * this.m_CellSize, 10000f, 1.5f * this.m_CellSize));
                netIterator.m_EdgeGeometryData = this.m_EdgeGeometryData;
                netIterator.m_LandValueData = this.m_LandValueData;
                NetIterator iterator = netIterator;
                this.m_NetSearchTree.Iterate(ref iterator);
                //float num = GroundPollutionSystem.GetPollution(cellCenter, this.m_GroundPollutionMap).m_Pollution;
                //float num2 = AirPollutionSystem.GetPollution(cellCenter, this.m_AirPollutionMap).m_Pollution;
                //float num3 = NoisePollutionSystem.GetPollution(cellCenter, this.m_NoisePollutionMap).m_Pollution;
                //float x = AvailabilityInfoToGridSystem.GetAvailabilityInfo(cellCenter, this.m_AvailabilityInfoMap).m_AvailabilityInfo.x;
                //float num4 = TelecomCoverage.SampleNetworkQuality(this.m_TelecomCoverageMap, cellCenter);
                LandValueCell value = this.m_LandValueMap[index];
                float num5 = (((float)iterator.m_TotalCount > 0f) ? (iterator.m_TotalLandValueBonus / (float)iterator.m_TotalCount) : 0f);
                //float num6 = math.min((x - 5f) * this.m_LandValueParameterData.m_AttractivenessBonusMultiplier, this.m_LandValueParameterData.m_CommonFactorMaxBonus);
                //float num7 = math.min(num4 * this.m_LandValueParameterData.m_TelecomCoverageBonusMultiplier, this.m_LandValueParameterData.m_CommonFactorMaxBonus);
                //num5 += num6 + num7;
                //float num8 = WaterUtils.SamplePolluted(ref this.m_WaterSurfaceData, cellCenter);
                //float num9 = 0f;
                //if (num8 <= 0f && num <= 0f)
                //{
                //	num9 = TerrainAttractivenessSystem.EvaluateAttractiveness(TerrainUtils.SampleHeight(ref this.m_TerrainHeightData, cellCenter), this.m_AttractiveMap[index], this.m_AttractivenessParameterData);
                //	num5 += math.min(math.max(num9 - 5f, 0f) * this.m_LandValueParameterData.m_AttractivenessBonusMultiplier, this.m_LandValueParameterData.m_CommonFactorMaxBonus);
                //}
                //float num10 = num * this.m_LandValueParameterData.m_GroundPollutionPenaltyMultiplier + num2 * this.m_LandValueParameterData.m_AirPollutionPenaltyMultiplier + num3 * this.m_LandValueParameterData.m_NoisePollutionPenaltyMultiplier;
                //float num11 = math.max(this.m_LandValueParameterData.m_LandValueBaseline, this.m_LandValueParameterData.m_LandValueBaseline + num5 - num10);
                float num11 = math.max(this.m_LandValueParameterData.m_LandValueBaseline, this.m_LandValueParameterData.m_LandValueBaseline + num5);
                if (math.abs(value.m_LandValue - num11) >= 0.1f)
                {
                    value.m_LandValue = math.lerp(value.m_LandValue, num11, 0.4f);
                }
                this.m_LandValueMap[index] = value;
            }
        }

        [BurstCompile]
		private struct EdgeUpdateJob : IJobChunk 
		{
			[ReadOnly]
			public EntityTypeHandle m_EntityType;

			[ReadOnly]
			public ComponentTypeHandle<Edge> m_EdgeType;

			[ReadOnly]
			public BufferTypeHandle<Game.Net.ServiceCoverage> m_ServiceCoverageType;

			[ReadOnly]
			public BufferTypeHandle<ResourceAvailability> m_AvailabilityType;

			[NativeDisableParallelForRestriction]
			public ComponentLookup<LandValue> m_LandValues;

			[ReadOnly]
			public LandValueParameterData m_LandValueParameterData;

            //add original lv mechnism;
            [ReadOnly]
            public BufferTypeHandle<ConnectedBuilding> m_ConnectedBuildingType;           

            [ReadOnly]
            public ComponentTypeHandle<Curve> m_CurveType;          

            [ReadOnly]
            public BufferLookup<Renter> m_RenterBuffers;

            [ReadOnly]
            public ComponentLookup<PropertyRenter> m_PropertyRenters;

            [ReadOnly]
            public ComponentLookup<PrefabRef> m_Prefabs;

            [ReadOnly]
            public ComponentLookup<BuildingData> m_BuildingDatas;

            [ReadOnly]
            public ComponentLookup<Abandoned> m_Abandoneds;

            [ReadOnly]
            public ComponentLookup<Destroyed> m_Destroyeds;

            [ReadOnly]
            public ComponentLookup<ConsumptionData> m_ConsumptionDatas;

            [ReadOnly]
            public ComponentLookup<BuildingPropertyData> m_PropertyDatas;

            [ReadOnly]
            public ComponentLookup<Household> m_Households;

            [ReadOnly]
            public ComponentLookup<Placeholder> m_Placeholders;

            [ReadOnly]
            public ComponentLookup<Attached> m_Attached;

            [ReadOnly]
            public BufferLookup<global::Game.Areas.SubArea> m_SubAreas;

            [ReadOnly]
            public ComponentLookup<global::Game.Areas.Lot> m_Lots;

            [ReadOnly]
            public ComponentLookup<Geometry> m_Geometries;

            //add ;
            [ReadOnly]
            public ComponentLookup<OfficeBuilding> m_OfficeBuilding;

            [ReadOnly]
            public ComponentLookup<ZonePropertiesData> m_ZonePropertyDatas;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
			{
				NativeArray<Entity> nativeArray = chunk.GetNativeArray(this.m_EntityType);
				NativeArray<Edge> nativeArray2 = chunk.GetNativeArray(ref this.m_EdgeType);
				BufferAccessor<Game.Net.ServiceCoverage> bufferAccessor = chunk.GetBufferAccessor(ref this.m_ServiceCoverageType);
				BufferAccessor<ResourceAvailability> bufferAccessor2 = chunk.GetBufferAccessor(ref this.m_AvailabilityType);
                NativeArray<Curve> nativeArray3 = chunk.GetNativeArray(ref this.m_CurveType);//add ori;
                BufferAccessor<ConnectedBuilding> bufferAccessor3 = chunk.GetBufferAccessor(ref this.m_ConnectedBuildingType);//add ori;
                for (int i = 0; i < nativeArray2.Length; i++)
				{
					Entity entity = nativeArray[i];
					LandValue value = this.m_LandValues[entity];
                    float num31 = 0f;
					float num32 = 0f;
					float num33 = 0f;
                    if (bufferAccessor.Length > 0)
					{
						DynamicBuffer<Game.Net.ServiceCoverage> dynamicBuffer = bufferAccessor[i];
						Game.Net.ServiceCoverage serviceCoverage = dynamicBuffer[0];
						num31 = math.lerp(serviceCoverage.m_Coverage.x, serviceCoverage.m_Coverage.y, 0.5f) * this.m_LandValueParameterData.m_HealthCoverageBonusMultiplier;
						Game.Net.ServiceCoverage serviceCoverage2 = dynamicBuffer[5];
						num32 = math.lerp(serviceCoverage2.m_Coverage.x, serviceCoverage2.m_Coverage.y, 0.5f) * this.m_LandValueParameterData.m_EducationCoverageBonusMultiplier;
						Game.Net.ServiceCoverage serviceCoverage3 = dynamicBuffer[2];
						num33 = math.lerp(serviceCoverage3.m_Coverage.x, serviceCoverage3.m_Coverage.y, 0.5f) * this.m_LandValueParameterData.m_PoliceCoverageBonusMultiplier;
					}
					float num34 = 0f;
					float num35 = 0f;
					float num36 = 0f;
					if (bufferAccessor2.Length > 0)
					{
						DynamicBuffer<ResourceAvailability> dynamicBuffer2 = bufferAccessor2[i];
						ResourceAvailability resourceAvailability = dynamicBuffer2[1];
						num34 = math.lerp(resourceAvailability.m_Availability.x, resourceAvailability.m_Availability.y, 0.5f) * this.m_LandValueParameterData.m_CommercialServiceBonusMultiplier;
						ResourceAvailability resourceAvailability2 = dynamicBuffer2[31];
						num35 = math.lerp(resourceAvailability2.m_Availability.x, resourceAvailability2.m_Availability.y, 0.5f) * this.m_LandValueParameterData.m_BusBonusMultiplier;
						ResourceAvailability resourceAvailability3 = dynamicBuffer2[32];
						num36 = math.lerp(resourceAvailability3.m_Availability.x, resourceAvailability3.m_Availability.y, 0.5f) * this.m_LandValueParameterData.m_TramSubwayBonusMultiplier;
                    }
					
                   
                    float num37 = math.max(num31 + num32 + num33 + num34 * 0.1f + num35 + num36, 0f);                    

                    //---add original lv mechnism;                    
                    Entity start = nativeArray2[i].m_Start;
                    Entity end = nativeArray2[i].m_End;
                    int num = 0;
                    float num2 = 0f;
                    int num3 = 0;
                    //float num14 = 0f;
                    DynamicBuffer<ConnectedBuilding> dynamicBuffer3 = bufferAccessor3[i];
                    for (int j = 0; j < dynamicBuffer3.Length; j++)
                    {
                        Entity building = dynamicBuffer3[j].m_Building;
                        if (this.m_Prefabs.HasComponent(building) && !this.m_Placeholders.HasComponent(building))
                        {
                            Entity prefab = this.m_Prefabs[building].m_Prefab;
                            if (!this.m_PropertyDatas.HasComponent(prefab) || this.m_Abandoneds.HasComponent(building) || this.m_Destroyeds.HasComponent(building))
                            {
                                continue;
                            }
                            BuildingPropertyData buildingPropertyData = this.m_PropertyDatas[prefab];
                            if (buildingPropertyData.m_AllowedStored != Resource.NoResource)
                            {
                                continue;
                            }
                            BuildingData buildingData = this.m_BuildingDatas[prefab];
                            ConsumptionData consumptionData = this.m_ConsumptionDatas[prefab];
                            //ZonePropertiesData zonePropertiesData = this.m_ZonePropertyDatas[entity];

                            int num5 = buildingPropertyData.CountProperties();

                            //mix res;
                            bool flag = buildingPropertyData.m_ResidentialProperties > 0 && (buildingPropertyData.m_AllowedSold != Resource.NoResource || buildingPropertyData.m_AllowedManufactured != Resource.NoResource);
                            int num6 = buildingData.m_LotSize.x * buildingData.m_LotSize.y;
                            
                            /*if (this.m_Attached.HasComponent(building))
                            {
                                Entity parent = this.m_Attached[building].m_Parent;
                                if (this.m_SubAreas.HasBuffer(parent))
                                {                                    
                                    DynamicBuffer<Game.Areas.SubArea> subArea = this.m_SubAreas[parent];
                                    num6 = Mathf.CeilToInt(num6 + ExtractorAISystem.GetArea(subArea, this.m_Lots, this.m_Geometries));
                                }
                            }*/
                            float num7 = value.m_LandValue * (float)num6 / (float)math.max(1, num5);
                            float num8 = (float)consumptionData.m_Upkeep / (float)math.max(1, num5);
                            if (this.m_RenterBuffers.HasBuffer(building))
                            {
                                DynamicBuffer<Renter> dynamicBuffer4 = this.m_RenterBuffers[building];
                                for (int k = 0; k < dynamicBuffer4.Length; k++)
                                {
                                    Entity renter = dynamicBuffer4[k].m_Renter;
                                    if (this.m_PropertyRenters.HasComponent(renter))
                                    {
                                        PropertyRenter propertyRenter = m_PropertyRenters[renter];                                       
                                        int maxrent = propertyRenter.m_MaxRent;
                                        //num2地价更新因素:建筑最大租金-维护费，若余额大于3倍当前地价则增加1，反之减少1；
                                        //其中混合建筑维护费和地价按0.4计算,增减量按0.4倍户数计算；
                                        //计算每户num2得到累计值；
                                        //New feature to calculate landvalue separately for each zonetype by simply change the result by modify MaxRent reference;
                                        //method may extened by cap the max landvalue; 
                                        //Avoid to modifiy MaxRent method directly for better compatibility;
                                        float resmixf = 0.8f * maxrent;//住商住工混合;
                                        if (!flag || this.m_Households.HasComponent(renter))
                                        {
                                            float residentlowf = 0.3f * maxrent;//低密住宅（1户）
                                            //float residentmediumf = 0.5f * maxrent;//中密度住宅；
                                            float residenthighf = 0.7f * maxrent;//中高密住宅;(不含住商住工混合);
                                            float commeriallf = 0.5f * maxrent;//低密商业；
                                            float commerialhf = 0.6f * maxrent;//高密商业；

                                            float manufacturf = 0.15f * maxrent;//制造业；
                                            float officehf = 0.08f * maxrent;//办公；
                                            float officelf = 0.1f * maxrent;//办公；
                                            float extractorf = 0.2f * maxrent;//采集业；
                                            //float storagef = 1f * maxrent;//仓储业；
                                            
                                            if (buildingPropertyData.m_ResidentialProperties > 0)
                                            {
                                                //低密住宅（资产属性值为1）;调低以抑制地价提升适用度；
                                                if (buildingPropertyData.m_ResidentialProperties == 1)
                                                {
                                                    num2 = (float)(num2 + ((residentlowf - num8 >= 3f * num7) ? 1f : (-1f)));                                                
                                                }
                                                //中高密住宅（不含住商混）；略微调低以抑制高租金住宅地价；
                                                else if (!flag)
                                                {
                                                    num2 = (float)(num2 + ((residenthighf - num8 >= 3f * num7) ? 1f : (-1f)));
                                                }                                                
                                                //住商混合；略微调低以抑制高租金地价；                                               
                                            }
                                            //低商业；大幅调低(维护费过低)
                                            if (buildingPropertyData.m_AllowedSold != Resource.NoResource && buildingPropertyData.m_ResidentialProperties <= 0 && buildingPropertyData.m_SpaceMultiplier == 1)
                                            {
                                                num2 = (float)(num2 + ((commeriallf - num8 >= 3f * num7) ? 1f : (-1f)));
                                            }
                                            //高商业；略微调低以抑制高利润商业地价；
                                            if (buildingPropertyData.m_AllowedSold != Resource.NoResource && buildingPropertyData.m_ResidentialProperties <= 0 && buildingPropertyData.m_SpaceMultiplier > 1)
                                            {
                                                num2 = (float)(num2 + ((commerialhf - num8 >= 3f * num7) ? 1f : (-1f)));
                                            }
                                            //工业；
                                            if (buildingPropertyData.m_AllowedManufactured != Resource.NoResource && buildingPropertyData.m_ResidentialProperties <= 0)
                                            //IndustrialProcessData process = this.m_IndustrialProcessDatas[prefab];
                                            //办公；大幅调低以抑制超高利润办公地价；may not suitable for RealEco；
                                            {
                                                if (this.m_OfficeBuilding.HasComponent(prefab))
                                                {
                                                    //propertyRenter.m_MaxRent = math.min(10000, propertyRenter.m_MaxRent);
                                                    if (buildingPropertyData.m_SpaceMultiplier > 2) //high office;
                                                    { num2 = (float)(num2 + ((officehf - num8 >= 3f * num7) ? 1f : (-1f))); }
                                                    else //low office;
                                                        num2 = (float)(num2 + ((officelf - num8 >= 3f * num7) ? 1f : (-1f)));
                                                }
                                                //采集业；地价影响不大；
                                                if (this.m_Attached.HasComponent(building))
                                                {
                                                    num2 = (float)(num2 + ((extractorf - num8 >= 3f * num7) ? 1f : (-1f)));
                                                }
                                                //仓储业；
                                                //if(buildingPropertyData.m_AllowedStore != Resource.NoResource)
                                                //{
                                                //    num2 = 0f;
                                                //}
                                                //制造业；已取消土地污染影响，调低以避免高地价；
                                                num2 = (float)(num2 + ((manufacturf - num8 >= 3f * num7) ? 1f : (-1f))); 
                                            }
                                        }
                                        else
                                        {
                                            float mixcompanyrent = 0.4f;
                                            num2 = (float)(num2 + ((resmixf - (mixcompanyrent * num8) >= 3f * mixcompanyrent * num7) ? Mathf.RoundToInt(mixcompanyrent * buildingPropertyData.m_ResidentialProperties) : -Mathf.RoundToInt(mixcompanyrent * buildingPropertyData.m_ResidentialProperties)));
                                            
                                        }                                        
                                        //***计算租户总数num3；
                                        num3++;
                                    }
                                }
                                num += num6;
                                int num9 = num5 - dynamicBuffer4.Length;
                                num2 -= (float)num9;
                                num3 += num9;
                            }                           
                        }
                        else
                        {
                            num++;
                        }
                    }
                    
                    float length = nativeArray3[i].m_Length;
                   
                    float distanceFade = LandValueSystemRe.GetDistanceFade(length);
                    
                    //num10 = num;
                    int num10 = math.max(num, Mathf.CeilToInt(length / 4f));
                    //num10 = (int)math.max(num, (length / 4f));
                    //num3 -= num - num10;
                    num3 -= num - num10;
                    
                    float2 @float = new float2(math.max(1f, this.m_LandValues[start].m_Weight), math.max(1f, this.m_LandValues[end].m_Weight));
                    float num11 = @float.x + @float.y;
                    float2 float2 = new float2(this.m_LandValues[start].m_LandValue, this.m_LandValues[end].m_LandValue);
                    //@float *= distanceFade;
                    @float *= distanceFade;//both x & y,but x is not used;so maybe they considered this but forgot later;
                    float y = 0f;
                    //y = math.lerp(float2.x, float2.y, @float.y / num11);vanilla
                    if (float2.y >= float2.x)//Fix;
                    {
                        y = math.lerp(float2.x, float2.y, @float.y / num11);
                    }
                    if (float2.y < float2.x)//Fix;
                    {
                        y = math.lerp(float2.y, float2.x, @float.x / num11);
                    }

                    //add service coverage lv factor;
                    num2 += num37/math.max(value.m_LandValue, 1f) * 8f;

                    float num12 = 0f;
                    if (num3 > 0)
                    {
                        num12 = 0.1f * num2 / (float)num3;
                    }
                   
                    value.m_Weight = math.max(1f, math.lerp(value.m_Weight, num10, 0.1f));//
                    //float s = num11 / (99f * value.m_Weight + num11);
                    float s = num11 / (99f * value.m_Weight + num11);
                    value.m_LandValue = math.lerp(value.m_LandValue, y, s);
                    
                    value.m_LandValue += math.min(1f, math.max(-2f, num12));
                    value.m_LandValue = math.max(value.m_LandValue, 0f);
                    value.m_Weight = math.lerp(value.m_Weight, math.max(1f, 0.5f * num11), s);
                    
                    //test!;测试是否生效；
                    //value.m_LandValue = 1500f;

                    //add lv cap to 1000f;
                    //value.m_LandValue = math.min(value.m_LandValue, 3000f);

                    //debug;set lv weight=0;
                    //value.m_Weight = 0;

                    this.m_LandValues[entity] = value;
                }
            }

            void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
			{
				this.Execute(in chunk, unfilteredChunkIndex, useEnabledMask, in chunkEnabledMask);
			}
        }
        
        [BurstCompile]
        private struct NodeUpdateJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;

            [ReadOnly]
            public ComponentTypeHandle<global::Game.Net.Node> m_NodeType;

            [ReadOnly]
            public BufferTypeHandle<ConnectedEdge> m_ConnectedEdgeType;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<LandValue> m_LandValues;

            [ReadOnly]
            public ComponentLookup<Curve> m_Curves;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> nativeArray;
                nativeArray = chunk.GetNativeArray(this.m_EntityType);
                NativeArray<global::Game.Net.Node> nativeArray2;
                nativeArray2 = chunk.GetNativeArray(ref this.m_NodeType);
                BufferAccessor<ConnectedEdge> bufferAccessor;
                bufferAccessor = chunk.GetBufferAccessor(ref this.m_ConnectedEdgeType);
                for (int i = 0; i < nativeArray2.Length; i++)
                {
                    Entity entity = nativeArray[i];
                    float num = 0f;
                    float num2 = 0f;
                    DynamicBuffer<ConnectedEdge> dynamicBuffer = bufferAccessor[i];
                    for (int j = 0; j < dynamicBuffer.Length; j++)
                    {
                        Entity edge = dynamicBuffer[j].m_Edge;
                        if (this.m_LandValues.HasComponent(edge))
                        {
                            float landValue = this.m_LandValues[edge].m_LandValue;
                            float num3 = this.m_LandValues[edge].m_Weight;
                            if (this.m_Curves.HasComponent(edge))
                            {
                                num3 *= LandValueSystemRe.GetDistanceFade(this.m_Curves[edge].m_Length);
                            }
                            if (landValue > 0)
                            {
                                num += landValue * num3;
                                num2 += num3;
                            }
                        }
                    }
                    if (num2 != 0f)
                    {
                        num /= num2;
                        LandValue value = this.m_LandValues[entity];
                        value.m_LandValue = math.lerp(value.m_LandValue, num, 0.05f);
                        value.m_Weight = math.max(1f, math.lerp(value.m_Weight, num2 / (float)dynamicBuffer.Length, 0.05f));
                        this.m_LandValues[entity] = value;
                    }
                }
            }

            void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                this.Execute(in chunk, unfilteredChunkIndex, useEnabledMask, in chunkEnabledMask);
            }
        }


        private struct TypeHandle
		{
			[ReadOnly]
			public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;

			[ReadOnly]
			public ComponentTypeHandle<Edge> __Game_Net_Edge_RO_ComponentTypeHandle;

			[ReadOnly]
			public BufferTypeHandle<Game.Net.ServiceCoverage> __Game_Net_ServiceCoverage_RO_BufferTypeHandle;

			[ReadOnly]
			public BufferTypeHandle<ResourceAvailability> __Game_Net_ResourceAvailability_RO_BufferTypeHandle;

			public ComponentLookup<LandValue> __Game_Net_LandValue_RW_ComponentLookup;

			[ReadOnly]
			public ComponentLookup<LandValue> __Game_Net_LandValue_RO_ComponentLookup;

			[ReadOnly]
			public ComponentLookup<EdgeGeometry> __Game_Net_EdgeGeometry_RO_ComponentLookup;

            //add original;
            [ReadOnly]
            public ComponentTypeHandle<Curve> __Game_Net_Curve_RO_ComponentTypeHandle;

            [ReadOnly]
            public BufferTypeHandle<ConnectedBuilding> __Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle;

            [ReadOnly]
            public ComponentLookup<BuildingData> __Game_Prefabs_BuildingData_RO_ComponentLookup;

            [ReadOnly]
            public ComponentLookup<global::Game.Objects.Transform> __Game_Objects_Transform_RO_ComponentLookup;

            [ReadOnly]
            public ComponentLookup<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentLookup;

            [ReadOnly]
            public ComponentLookup<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentLookup;

            [ReadOnly]
            public BufferLookup<Renter> __Game_Buildings_Renter_RO_BufferLookup;

            [ReadOnly]
            public ComponentLookup<Abandoned> __Game_Buildings_Abandoned_RO_ComponentLookup;

            [ReadOnly]
            public ComponentLookup<Destroyed> __Game_Common_Destroyed_RO_ComponentLookup;

            [ReadOnly]
            public ComponentLookup<ConsumptionData> __Game_Prefabs_ConsumptionData_RO_ComponentLookup;

            [ReadOnly]
            public ComponentLookup<BuildingPropertyData> __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;

            [ReadOnly]
            public ComponentLookup<Household> __Game_Citizens_Household_RO_ComponentLookup;

            [ReadOnly]
            public ComponentLookup<Placeholder> __Game_Objects_Placeholder_RO_ComponentLookup;

            [ReadOnly]
            public ComponentLookup<Attached> __Game_Objects_Attached_RO_ComponentLookup;

            [ReadOnly]
            public BufferLookup<global::Game.Areas.SubArea> __Game_Areas_SubArea_RO_BufferLookup;

            [ReadOnly]
            public ComponentLookup<global::Game.Areas.Lot> __Game_Areas_Lot_RO_ComponentLookup;

            [ReadOnly]
            public ComponentLookup<Geometry> __Game_Areas_Geometry_RO_ComponentLookup;

            [ReadOnly]
            public ComponentTypeHandle<global::Game.Net.Node> __Game_Net_Node_RO_ComponentTypeHandle;

            [ReadOnly]
            public BufferTypeHandle<ConnectedEdge> __Game_Net_ConnectedEdge_RO_BufferTypeHandle;

            [ReadOnly]
            public ComponentLookup<Curve> __Game_Net_Curve_RO_ComponentLookup;

            //add office;
            [ReadOnly]
            public ComponentLookup<OfficeBuilding> __Game_Prefabs_OfficeBuilding_RO_ComponentLookup;
            //add zoneprefab;
            [ReadOnly]
            public ComponentLookup<ZonePropertiesData> __Game_Prefabs_ZonePropertiesData_RO_ComponentLookup;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void __AssignHandles(ref SystemState state)
			{
				this.__Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
				this.__Game_Net_Edge_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Edge>(isReadOnly: true);
				this.__Game_Net_ServiceCoverage_RO_BufferTypeHandle = state.GetBufferTypeHandle<Game.Net.ServiceCoverage>(isReadOnly: true);
				this.__Game_Net_ResourceAvailability_RO_BufferTypeHandle = state.GetBufferTypeHandle<ResourceAvailability>(isReadOnly: true);
				this.__Game_Net_LandValue_RW_ComponentLookup = state.GetComponentLookup<LandValue>();
				this.__Game_Net_LandValue_RO_ComponentLookup = state.GetComponentLookup<LandValue>(isReadOnly: true);
				this.__Game_Net_EdgeGeometry_RO_ComponentLookup = state.GetComponentLookup<EdgeGeometry>(isReadOnly: true);
                //add original;
                this.__Game_Net_Curve_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Curve>(isReadOnly: true);
                this.__Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle = state.GetBufferTypeHandle<ConnectedBuilding>(isReadOnly: true);
                this.__Game_Prefabs_BuildingData_RO_ComponentLookup = state.GetComponentLookup<BuildingData>(isReadOnly: true);
                this.__Game_Objects_Transform_RO_ComponentLookup = state.GetComponentLookup<global::Game.Objects.Transform>(isReadOnly: true);
                this.__Game_Prefabs_PrefabRef_RO_ComponentLookup = state.GetComponentLookup<PrefabRef>(isReadOnly: true);
                this.__Game_Buildings_PropertyRenter_RO_ComponentLookup = state.GetComponentLookup<PropertyRenter>(isReadOnly: true);
                this.__Game_Buildings_Renter_RO_BufferLookup = state.GetBufferLookup<Renter>(isReadOnly: true);
                this.__Game_Buildings_Abandoned_RO_ComponentLookup = state.GetComponentLookup<Abandoned>(isReadOnly: true);
                this.__Game_Common_Destroyed_RO_ComponentLookup = state.GetComponentLookup<Destroyed>(isReadOnly: true);
                this.__Game_Prefabs_ConsumptionData_RO_ComponentLookup = state.GetComponentLookup<ConsumptionData>(isReadOnly: true);
                this.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup = state.GetComponentLookup<BuildingPropertyData>(isReadOnly: true);
                this.__Game_Citizens_Household_RO_ComponentLookup = state.GetComponentLookup<Household>(isReadOnly: true);
                this.__Game_Objects_Placeholder_RO_ComponentLookup = state.GetComponentLookup<Placeholder>(isReadOnly: true);
                this.__Game_Objects_Attached_RO_ComponentLookup = state.GetComponentLookup<Attached>(isReadOnly: true);
                this.__Game_Areas_SubArea_RO_BufferLookup = state.GetBufferLookup<global::Game.Areas.SubArea>(isReadOnly: true);
                this.__Game_Areas_Lot_RO_ComponentLookup = state.GetComponentLookup<global::Game.Areas.Lot>(isReadOnly: true);
                this.__Game_Areas_Geometry_RO_ComponentLookup = state.GetComponentLookup<Geometry>(isReadOnly: true);
                this.__Game_Net_Node_RO_ComponentTypeHandle = state.GetComponentTypeHandle<global::Game.Net.Node>(isReadOnly: true);
                this.__Game_Net_ConnectedEdge_RO_BufferTypeHandle = state.GetBufferTypeHandle<ConnectedEdge>(isReadOnly: true);
                this.__Game_Net_Curve_RO_ComponentLookup = state.GetComponentLookup<Curve>(isReadOnly: true);
                //add office;
                this.__Game_Prefabs_OfficeBuilding_RO_ComponentLookup = state.GetComponentLookup<OfficeBuilding>(isReadOnly: true);
                //add zoneprefab;
                this.__Game_Prefabs_ZonePropertiesData_RO_ComponentLookup = state.GetComponentLookup<ZonePropertiesData>(isReadOnly: true);

            }
		}

		public static readonly int kTextureSize = 128;

		public static readonly int kUpdatesPerDay = 32;

		private EntityQuery m_EdgeGroup;

		private EntityQuery m_NodeGroup;

		private EntityQuery m_LandValueParameterQuery;

		private Game.Net.SearchSystem m_NetSearchSystem;

		private TypeHandle __TypeHandle;

        private static float GetDistanceFade(float distance)
        {
            return math.saturate((float)(1 - (double)distance / 500f));
        }

        public int2 TextureSize => new int2(LandValueSystemRe.kTextureSize, LandValueSystemRe.kTextureSize);

		public override int GetUpdateInterval(SystemUpdatePhase phase)
		{
            return 262144 / LandValueSystemRe.kUpdatesPerDay;
            //return 8;//test only;
        }

		public static float3 GetCellCenter(int index)
		{
			return CellMapSystem<LandValueCell>.GetCellCenter(index, LandValueSystemRe.kTextureSize);
		}

		public static int GetCellIndex(float3 pos)
		{
			int num = CellMapSystem<LandValueCell>.kMapSize / LandValueSystemRe.kTextureSize;
			return Mathf.FloorToInt(((float)(CellMapSystem<LandValueCell>.kMapSize / 2) + pos.x) / (float)num) + Mathf.FloorToInt(((float)(CellMapSystem<LandValueCell>.kMapSize / 2) + pos.z) / (float)num) * LandValueSystemRe.kTextureSize;
		}

		[Preserve]
		protected override void OnCreate()
		{
			base.OnCreate();
            //log;
            Mod.log.Info("LandValueSystemRe Oncreated!");
            //Assert.IsTrue(LandValueSystem.kTextureSize == TerrainAttractivenessSystem.kTextureSize);
			base.CreateTextures(LandValueSystemRe.kTextureSize);
			this.m_NetSearchSystem = base.World.GetOrCreateSystemManaged<Game.Net.SearchSystem>();
			//this.m_GroundPollutionSystem = base.World.GetOrCreateSystemManaged<GroundPollutionSystem>();
			//this.m_AirPollutionSystem = base.World.GetOrCreateSystemManaged<AirPollutionSystem>();
			//this.m_NoisePollutionSystem = base.World.GetOrCreateSystemManaged<NoisePollutionSystem>();
			//this.m_TerrainAttractivenessSystem = base.World.GetOrCreateSystemManaged<TerrainAttractivenessSystem>();
			//this.m_AvailabilityInfoToGridSystem = base.World.GetOrCreateSystemManaged<AvailabilityInfoToGridSystem>();
			//this.m_TerrainSystem = base.World.GetOrCreateSystemManaged<TerrainSystem>();
			//this.m_WaterSystem = base.World.GetOrCreateSystemManaged<WaterSystem>();
			//this.m_TelecomCoverageSystem = base.World.GetOrCreateSystemManaged<TelecomCoverageSystem>();
			//this.m_AttractivenessParameterQuery = base.GetEntityQuery(ComponentType.ReadOnly<AttractivenessParameterData>());
			this.m_LandValueParameterQuery = base.GetEntityQuery(ComponentType.ReadOnly<LandValueParameterData>());
			this.m_EdgeGroup = base.GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[4]
				{
					ComponentType.ReadOnly<Edge>(),
					ComponentType.ReadWrite<LandValue>(),
					ComponentType.ReadOnly<Curve>(),
                    ComponentType.ReadOnly<ConnectedBuilding>()
                },
				Any = new ComponentType[0],
				None = new ComponentType[2]
				{
					ComponentType.ReadOnly<Deleted>(),
					ComponentType.ReadOnly<Temp>()
				}
			});
            this.m_NodeGroup = this.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[3]
       {
          ComponentType.ReadOnly<Game.Net.Node>(),
          ComponentType.ReadWrite<LandValue>(),
          ComponentType.ReadOnly<ConnectedEdge>()
       },
                Any = new ComponentType[0],
                None = new ComponentType[2]
       {
          ComponentType.ReadOnly<Deleted>(),
          ComponentType.ReadOnly<Temp>()
       }
            });
            base.RequireAnyForUpdate(this.m_EdgeGroup, this.m_NodeGroup);
		}

		[Preserve]
		protected override void OnUpdate()
		{
            //add ori;
            //JobHandle jobHandle;
            //jobHandle = base.Dependency;
            if (!this.m_EdgeGroup.IsEmptyIgnoreFilter)
			{
				
                //add ori:
                this.__TypeHandle.__Game_Areas_Geometry_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Areas_Lot_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Areas_SubArea_RO_BufferLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Objects_Attached_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Objects_Placeholder_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Citizens_Household_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Common_Destroyed_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_Abandoned_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_Renter_RO_BufferLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_LandValue_RW_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Objects_Transform_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_Curve_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_Edge_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_OfficeBuilding_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_ResourceAvailability_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_ServiceCoverage_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
                EdgeUpdateJob edgeUpdateJob = default(EdgeUpdateJob);
				edgeUpdateJob.m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle;
				edgeUpdateJob.m_EdgeType = this.__TypeHandle.__Game_Net_Edge_RO_ComponentTypeHandle;
				edgeUpdateJob.m_ServiceCoverageType = this.__TypeHandle.__Game_Net_ServiceCoverage_RO_BufferTypeHandle;
				edgeUpdateJob.m_AvailabilityType = this.__TypeHandle.__Game_Net_ResourceAvailability_RO_BufferTypeHandle;
				edgeUpdateJob.m_LandValueParameterData = this.m_LandValueParameterQuery.GetSingleton<LandValueParameterData>();
                //add ori;
                edgeUpdateJob.m_CurveType = this.__TypeHandle.__Game_Net_Curve_RO_ComponentTypeHandle;
                edgeUpdateJob.m_ConnectedBuildingType = this.__TypeHandle.__Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle;
                edgeUpdateJob.m_BuildingDatas = this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup;
                edgeUpdateJob.m_LandValues = this.__TypeHandle.__Game_Net_LandValue_RW_ComponentLookup;
                edgeUpdateJob.m_Prefabs = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup;
                edgeUpdateJob.m_PropertyRenters = this.__TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup;
                edgeUpdateJob.m_RenterBuffers = this.__TypeHandle.__Game_Buildings_Renter_RO_BufferLookup;
                edgeUpdateJob.m_Abandoneds = this.__TypeHandle.__Game_Buildings_Abandoned_RO_ComponentLookup;
                edgeUpdateJob.m_Destroyeds = this.__TypeHandle.__Game_Common_Destroyed_RO_ComponentLookup;
                edgeUpdateJob.m_ConsumptionDatas = this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup;
                edgeUpdateJob.m_PropertyDatas = this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
                edgeUpdateJob.m_Households = this.__TypeHandle.__Game_Citizens_Household_RO_ComponentLookup;
                edgeUpdateJob.m_Placeholders = this.__TypeHandle.__Game_Objects_Placeholder_RO_ComponentLookup;
                edgeUpdateJob.m_Attached = this.__TypeHandle.__Game_Objects_Attached_RO_ComponentLookup;
                edgeUpdateJob.m_SubAreas = this.__TypeHandle.__Game_Areas_SubArea_RO_BufferLookup;
                edgeUpdateJob.m_Lots = this.__TypeHandle.__Game_Areas_Lot_RO_ComponentLookup;
                edgeUpdateJob.m_Geometries = this.__TypeHandle.__Game_Areas_Geometry_RO_ComponentLookup;
                //add office;
                edgeUpdateJob.m_OfficeBuilding = this.__TypeHandle.__Game_Prefabs_OfficeBuilding_RO_ComponentLookup;
                //add zoneprefab;
                edgeUpdateJob.m_ZonePropertyDatas = this.__TypeHandle.__Game_Prefabs_ZonePropertiesData_RO_ComponentLookup;
                base.Dependency = edgeUpdateJob.ScheduleParallel(this.m_EdgeGroup, base.Dependency);
            }
            //add ori:
            
            if (!this.m_NodeGroup.IsEmptyIgnoreFilter)
            {
                this.__TypeHandle.__Game_Net_LandValue_RW_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_Curve_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_ConnectedEdge_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_Node_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
                NodeUpdateJob nodeUpdateJob;
                nodeUpdateJob = default(NodeUpdateJob);
                nodeUpdateJob.m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle;
                nodeUpdateJob.m_NodeType = this.__TypeHandle.__Game_Net_Node_RO_ComponentTypeHandle;
                nodeUpdateJob.m_ConnectedEdgeType = this.__TypeHandle.__Game_Net_ConnectedEdge_RO_BufferTypeHandle;
                nodeUpdateJob.m_Curves = this.__TypeHandle.__Game_Net_Curve_RO_ComponentLookup;
                nodeUpdateJob.m_LandValues = this.__TypeHandle.__Game_Net_LandValue_RW_ComponentLookup;
                base.Dependency = nodeUpdateJob.ScheduleParallel(this.m_NodeGroup, base.Dependency);
            }
            this.__TypeHandle.__Game_Net_EdgeGeometry_RO_ComponentLookup.Update(ref base.CheckedStateRef);
			this.__TypeHandle.__Game_Net_LandValue_RO_ComponentLookup.Update(ref base.CheckedStateRef);
			LandValueMapUpdateJob landValueMapUpdateJob = default(LandValueMapUpdateJob);
            landValueMapUpdateJob.m_NetSearchTree = this.m_NetSearchSystem.GetNetSearchTree(readOnly: true, out JobHandle dependencies1);
            landValueMapUpdateJob.m_LandValueMap = base.m_Map;
			landValueMapUpdateJob.m_LandValueData = this.__TypeHandle.__Game_Net_LandValue_RO_ComponentLookup;
			landValueMapUpdateJob.m_EdgeGeometryData = this.__TypeHandle.__Game_Net_EdgeGeometry_RO_ComponentLookup;
			landValueMapUpdateJob.m_LandValueParameterData = this.m_LandValueParameterQuery.GetSingleton<LandValueParameterData>();
			landValueMapUpdateJob.m_CellSize = (float)CellMapSystem<LandValueCell>.kMapSize / (float)LandValueSystemRe.kTextureSize;
            LandValueMapUpdateJob jobData = landValueMapUpdateJob;
            
            base.Dependency = jobData.Schedule(LandValueSystemRe.kTextureSize * LandValueSystemRe.kTextureSize, LandValueSystemRe.kTextureSize, JobHandle.CombineDependencies(dependencies1, JobHandle.CombineDependencies(base.m_WriteDependencies, base.m_ReadDependencies, base.Dependency)));

            base.AddWriter(base.Dependency);
			this.m_NetSearchSystem.AddNetSearchTreeReader(base.Dependency);
			base.Dependency = JobHandle.CombineDependencies(base.m_ReadDependencies, base.m_WriteDependencies, base.Dependency);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void __AssignQueries(ref SystemState state)
		{
		}

		protected override void OnCreateForCompiler()
		{
			base.OnCreateForCompiler();
			this.__AssignQueries(ref base.CheckedStateRef);
			this.__TypeHandle.__AssignHandles(ref base.CheckedStateRef);
		}

		[Preserve]
		public LandValueSystemRe()
		{
		}

        
    }
}
