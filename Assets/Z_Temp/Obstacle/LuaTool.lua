-- function GetObstacleForStr(SceneName)
-- 	local _lg_shanmaiObstacle = require (SceneName)
-- 	if(_lg_shanmaiObstacle == nil) then 
-- 		print(SceneName ," Table is nil")
-- 		return
-- 	end
-- 	local xStr = ""
-- 	local zStr = ""
-- 	local PosX = _lg_shanmaiObstacle.PosX
-- 	local PosY = _lg_shanmaiObstacle.PosY
-- 	local PosZ = _lg_shanmaiObstacle.PosZ
-- 	for i=1,#PosX do
-- 		xStr = xStr .. tostring(PosX[i])..","
-- 		zStr = zStr .. tostring(PosZ[i])..","
-- 	end
-- 	_G.package.loaded[SceneName] = false
-- 	return _lg_shanmaiObstacle.MapWith, _lg_shanmaiObstacle.MapHeight, _lg_shanmaiObstacle.MapStartX, _lg_shanmaiObstacle.MapStartY,xStr,zStr;
-- end
require "Npc/funNpcServer"
require "Npc/npcServer"
require "Obj/IAObjectData"
require "Gac/GacScene/ScenePointSet"
require "Gac/Character/Npc/GacFunNpcLoader"
require "Gac/Character/Npc/GacNpcLoader"
require "Scene/RandomSceneBlock"

function LoadNpcMapInfo(sSceneName)
	local tInfo = require (sSceneName)
	local sIdInfo1 = ""
	for k,v in pairs(tInfo[1])do
		if sIdInfo1 == "" then
			sIdInfo1 = sIdInfo1..k
		else
			sIdInfo1 = sIdInfo1..","..k
		end
		
	end

	local sIdInfo2 = ""
	for k,v in pairs(tInfo[2]) do
		if sIdInfo2 == "" then
			sIdInfo2 = sIdInfo2..k
		else
			sIdInfo2 = sIdInfo2..","..k
		end
		
	end

	local sIdInfo3 = ""
	if tInfo[3] then
		for k,v in pairs(tInfo[3]) do
			if sIdInfo3 == "" then
				sIdInfo3 = sIdInfo3..k
			else
				sIdInfo3 = sIdInfo3..","..k
			end
			
		end
	end
	

	local sIdInfo4 = ""
	if tInfo[4] then
		for k,v in pairs(tInfo[4]) do
			if sIdInfo4 == "" then
				sIdInfo4 = sIdInfo4..k
			else
				sIdInfo4 = sIdInfo4..","..k
			end
			
		end
	end
	

	return sIdInfo1, tInfo[1], sIdInfo2, tInfo[2], sIdInfo3, tInfo[3] or {}, sIdInfo4, tInfo[4] or {}
end

function InitNpcInfo()
	local sId = ""
	for npcid, tInfo in pairs(NpcSet) do
		if sId == "" then
			sId = sId .. npcid
		else
			sId = sId .. "," .. npcid
		end
	end
	return sId, NpcSet
end

function InitFunNpcInfo()
	local sId = ""
	for npcid, tInfo in pairs(FunNpcSet) do
		if sId == "" then
			sId = sId .. npcid
		else
			sId = sId .. "," .. npcid
		end
	end
	return sId, FunNpcSet
end


function InitOjbInfo()
	local sId = ""
	for objid, tInfo in pairs(IAObjectData) do
		if sId == "" then
			sId = sId .. objid
		else
			sId = sId .. "," .. objid
		end
	end
	return sId, IAObjectData
end

function GetObstacleForStr(SceneName)
	local ObstacleInfo = require (SceneName)
	if(ObstacleInfo == nil) then 
		print(SceneName ," Table is nil")
		return
	end
	local xStr = ""
	local zStr = ""
	local ObstacleTable = ObstacleInfo.Obstacle
	for x,y in pairs(ObstacleTable) do
		xStr = xStr .. tostring(x) .. ","
		for _,yInfo in pairs(y) do
			zStr = zStr .. yInfo  .. "|"
		end
		zStr = zStr .. ","
	end
	_G.package.loaded[SceneName] = false
	return ObstacleInfo.MapWith, ObstacleInfo.MapHeight, ObstacleInfo.MapStartX, ObstacleInfo.MapStartY,xStr,zStr;
end

function GetMonsterInfoForStr(SceneName)
	local Scene = require (SceneName)
	if(Scene == nil) then 
		print(SceneName ," Table is nil")
		return
	end
	local xStr = ""
	local zStr = ""
	local idStr = ""
	local typestr = ""
	local rotationStr = ""
	local wayPoint = ""
	local repeatpath = ""
	local groupstr = 1
	local updatetimestr = ""
	local objidstr = 1

	for i=1,#Scene do
		local table = Scene[i]
		typestr = typestr .. tostring(table.type)..","
		idStr   = idStr .. tostring(table.id) .. ","
		rotationStr = rotationStr .. tostring(table.rotation)..","
		xStr = xStr .. tostring(table.posx)..","
		zStr = zStr .. tostring(table.posz)..","
		if(table.repeatpath) then
			repeatpath = repeatpath .. tostring(table.repeatpath) .. ","
		end
		if table.group then
			groupstr = groupstr .. tostring(table.group)..","
		else
			groupstr = groupstr .. tostring(-1)..","
		end

		if table.updatetime then
			updatetimestr = updatetimestr..tostring(table.updatetime) ..","
		else
			updatetimestr = updatetimestr..tostring(0) ..","
		end

		if table.objid then
			objidstr = objidstr .. tostring(table.objid) .. ","
		else
			objidstr = objidstr .. ","
		end

		
		if table.waytable then
			for _,v in ipairs(table.waytable) do
				wayPoint = wayPoint .. tostring(v) .. ";"
			end
		end
		wayPoint = wayPoint .. ","
	end
	_G.package.loaded[SceneName] = false
	return xStr,zStr,idStr,typestr,rotationStr,Scene.MapWith,Scene.MapHeight,Scene.MapStartX,Scene.MapStartY,wayPoint,repeatpath,groupstr,updatetimestr,objidstr
end

--获取怪物信息(已经弃用)
function GetMonsterInfo( )
	if(Character == nil) then 
		print("MonsterInfo Table is nil")
		return
	end

	local namestr = ""
	local Modelstr = ""
	local levelstr = ""
	local idStr = ""
	local atkTypeStr = ""

	for k,v in pairs(Character) do
		local table = Character[k]
		namestr = namestr .. tostring(table.name)..","
		idStr   = idStr .. tostring(k) .. ","
		Modelstr = Modelstr .. tostring(table.model)..","
		levelstr = levelstr .. tostring(table.level)..","
		atkTypeStr = atkTypeStr .. tostring(table.atkType)..","
	end
	return namestr, idStr, Modelstr, levelstr, atkTypeStr
end

--获取OBj信息(已经弃用)
function GetOBJInfo( )
	if(gtObjConfig == nil) then 
		print("OBJInfo Table is nil")
		return
	end

	local namestr = ""
	local idStr = ""

	for k,v in pairs(gtObjConfig) do
		local table = gtObjConfig[k]
		namestr = namestr .. tostring(table.name)..","
		idStr   = idStr .. tostring(k) .. ","
	end

	return namestr, idStr
end

function GetNavigateForStr( SceneName )
	local Scene = require (SceneName)
	if(Scene == nil) then 
		print(SceneName ," Table is nil")
		return
	end
	local NavigateStr = ""
	for i=1,#Scene do
		NavigateStr = NavigateStr .. tostring(Scene[i]) .. ","
	end
	return NavigateStr
end

function GetBlockInfo(sBlockResource)
	for nId, tBlock in pairs(RandomSceneBlock) do
		if tBlock.Resource == sBlockResource then
			return nId, tBlock.Width, tBlock.Height, tBlock.PixelWidth, tBlock.PixelHeight
		else

			local tName = Fastsplit("/", tBlock.Resource)
			if #tName == 2 then
				if tName[2] == sBlockResource then
					return nId, tBlock.Width, tBlock.Height, tBlock.PixelWidth, tBlock.PixelHeight		
				end
			end
		end		
	end
	return 0,0,0,0,0
end

function Fastsplit(sep, s)
	if not s or not sep or #s == 0 or #sep == 0 then
		return {}
	end

    local startpos  = 1
    --根据Convert特性，改回。废除将末尾的空字符也算入 ",,"将得到2个段
    local endpos    = #s -- + 1
    local rettbl    = {}
    while true do
        local findpos = string.find(s, sep, startpos)
        if findpos then
            table.insert(rettbl, string.sub(s, startpos, findpos - 1))
            startpos = findpos + 1
        else
            break
        end
    end
    --没超过字符串长度才分割后面的字符
    if startpos <= endpos then
    	table.insert(rettbl, string.sub(s, startpos, endpos))
    end

    return rettbl
end