import os
import math
import random
import networkx as nx
import matplotlib.pyplot as plt

### STRUCTS ##################################################################

class Room:
    originX = None
    originY = None
    endX = None
    endY = None
    isCorridor = None

### INPUT/OUTPUT FUNCTIONS ####################################################

# Gets the name of the map and get the files path.
def getFiles(inputDir):
    inputAcquired = False
    text = input("\nInsert the MAPNAME value: ")
    while inputAcquired == False:
        mapFileName = text + ".map.txt"
        ABFileName = text + ".AB.txt"
        mapFilePath = inputDir + "/" + mapFileName
        ABFilePath = inputDir + "/" + ABFileName
        if os.path.isfile(mapFilePath) and os.path.isfile(ABFilePath):
            print("Files found.\n")
            inputAcquired = True
        else:
            text = input("Files not found. Insert the MAPNAME value: ")
    return text, mapFileName, ABFileName, mapFilePath, ABFilePath

# Reads the map.
def readMap(filePath):
    print("Reading the map file... ", end='', flush=True)
    with open(filePath) as f:
        lines = [x.strip() for x in f.readlines()]
        map = [[lines[j][i] for i in range(len(lines[0]))] for j in range(len(lines))]
    print("Done.")
    return map

# Reads the AB file.
def readAB(filePath):
    print("Reading the AB file... ", end='', flush=True)
    with open(filePath) as f:
        genome = f.readline()

        rooms = []
        currentValue = ""
        currentChar = 0

        while currentChar < len(genome) and genome[currentChar] == "<":
            room = Room()
            room.isCorridor = False
            currentChar = currentChar + 1

            # Get the x coordinate of the origin.
            while genome[currentChar].isdigit():
                currentValue = currentValue + genome[currentChar]
                currentChar = currentChar + 1
            room.originX = int(currentValue)

            currentValue = ""
            currentChar = currentChar + 1

            # Get the y coordinate of the origin.
            while genome[currentChar].isdigit():
                currentValue = currentValue + genome[currentChar]
                currentChar = currentChar + 1
            room.originY = int(currentValue)

            currentValue = ""
            currentChar = currentChar + 1

            # Get the size of the arena.
            while genome[currentChar].isdigit():
                currentValue = currentValue + genome[currentChar]
                currentChar = currentChar + 1
            room.endX = int(room.originX) + int(currentValue) - 1
            room.endY = int(room.originY) + int(currentValue) - 1
            rooms.append(room)

            currentValue = ""
            currentChar = currentChar + 1

        if currentChar < len(genome) and genome[currentChar] == "|":
            currentChar = currentChar + 1

            while (currentChar < len(genome) and genome[currentChar] == "<"):
                room = Room()
                room.isCorridor = True
                currentChar = currentChar + 1

                # Get the x coordinate of the origin.
                while genome[currentChar].isdigit():
                    currentValue = currentValue + genome[currentChar]
                    currentChar = currentChar + 1
                room.originX = int(currentValue)

                currentValue = ""
                currentChar = currentChar + 1

                # Get the y coordinate of the origin.
                while genome[currentChar].isdigit():
                    currentValue = currentValue + genome[currentChar]
                    currentChar = currentChar + 1
                room.originY = int(currentValue)

                currentValue = ""
                currentChar = currentChar + 1

                # Get the length of the corridor.
                if genome[currentChar] == "-":
                    currentValue = currentValue + genome[currentChar]
                    currentChar = currentChar + 1        
                while genome[currentChar].isdigit():
                    currentValue = currentValue + genome[currentChar]
                    currentChar = currentChar + 1
                if int(currentValue) > 0:
                    room.endX = int(room.originX) + int(currentValue) - 1
                    room.endY = int(room.originY) + 3 - 1
                else:
                    room.endX = int(room.originX) + 3 - 1
                    room.endY = int(room.originY) - int(currentValue) - 1
                rooms.append(room)

                currentValue = ""
                currentChar = currentChar + 1

    print("Done.")
    return rooms

# Exports the map.
def exportMap(filePath):
    print("Exporting the map... ", end='', flush=True)

    mapString = ""

    for x in range(len(map)):
        for y in range(len(map[0])):
            mapString = mapString + map[x][y]
        if (x < len(map) - 1):
            mapString = mapString + "\n"

    file = open(filePath, "w")
    file.write(mapString)
    file.close()

    print("Done.")

# Merges AB rooms.
def mergeRooms(rooms):
    mergedCount = 0

    for room in rooms:
        if not room.isCorridor:
            nextRoom = next((nextRoom for nextRoom in rooms if (nextRoom.originX > room.originX and nextRoom.originX <= room.endX + 1 and \
                                                                nextRoom.originY == room.originY and nextRoom.endY == room.endY and \
                                                                nextRoom.endY - nextRoom.originY == room.endY - room.originY)), None)
            if nextRoom is not None and not nextRoom.isCorridor:
                # print("Merging room <" + str(room.originX) + "," +
                # str(room.originY) + ">" + "<" + str(room.endX) + "," +
                # str(room.endY) + ">" + " and <" + \
                #       str(nextRoom.originX) + "," + str(nextRoom.originY) +
                #       ">" + "<" + str(nextRoom.endX) + "," +
                #       str(nextRoom.endY) + ">.")
                room.endX = nextRoom.endX
                room.endY = nextRoom.endY
                rooms.remove(nextRoom)
                mergedCount = mergedCount + 1
            else:
                nextRoom = next((nextRoom for nextRoom in rooms if (nextRoom.originY > room.originY and nextRoom.originY <= room.endY + 1 and \
                                                                    nextRoom.originX == room.originX and nextRoom.endX == room.endX and \
                                                                    nextRoom.endX - nextRoom.originX == room.endX - room.originX)), None) 
                if nextRoom is not None and not nextRoom.isCorridor:
                    # print("Merging room <" + str(room.originX) + "," +
                    # str(room.originY) + ">" + "<" + str(room.endX) + "," +
                    # str(room.endY) + ">" + " and <" + \
                    #       str(nextRoom.originX) + "," + str(nextRoom.originY)
                    #       + ">" + "<" + str(nextRoom.endX) + "," +
                    #       str(nextRoom.endY) + ">.")
                    room.endX = nextRoom.endX
                    room.endY = nextRoom.endY
                    rooms.remove(nextRoom)
                    mergedCount = mergedCount + 1
    
    if mergedCount == 0:
        return
    else:
        mergeRooms(rooms)

# Removes useless rooms.
def removeRooms(rooms):
    toBeRemoved = []

    for r1 in rooms:
        for r2 in rooms:
            if r1 is not r2 and r1.originX <= r2.originX and r1.originY <= r2.originY and r1.endX >= r2.endX and r1.endY >= r2.endY:
                toBeRemoved.append(r2)

    return [r for r in rooms if r not in toBeRemoved]

### GENERATION FUNCTIONS ######################################################

# Adds all the objects to the map.
def addEverything(map, rooms, spawnPoint, medkit, ammo):
    width = len(map)
    height = len(map[0])

    # Removing the objects.
    print("\nRemoving the pre-existing objects... ", end='', flush=True)
    for x in range(width): 
        for y in range(height): 
            if not map[x][y] == "w" and not map[x][y] == "r" :
                map[x][y] = "r"
    print("Done.")

    print("Initializing the variables... ", end='', flush=True)

    roomGraph = getRoomsCorridorsGraph(rooms, False)
    diameter = getDiameterLength(roomGraph)
    diagonal = math.sqrt(math.pow(width, 2) + math.pow(height, 2))

    visibilityMatrix = getVisibilityMatrix(map)
    normalizedDegree = getNormalizedDegree(roomGraph)
    placedObjects = []

    print("Done.")

    # Place the spawn points.
    print("Placing the spawn points... ", end='', flush=True)

    degreeFit = getNormalizedDegreeFit(normalizedDegree, 0.1, 0.3)
    visibilityFit = [[(1 - visibilityMatrix[x][y]) for y in range(len(visibilityMatrix[0]))] for x in range(len(visibilityMatrix))]

    for i in range(spawnPoint[1]):
        bestTile = getBestTile(roomGraph, diameter, diagonal, spawnPoint, [spawnPoint[0]], placedObjects, degreeFit, visibilityFit, [1, 0.25, -2], [1, 0.5, 0.5])
        addResource(bestTile[0], bestTile[1], spawnPoint[0], roomGraph, map)
        placedObjects.append([bestTile[0], bestTile[1], spawnPoint[0]])

    print("Done.")

    # Place the medkits.
    print("Placing the medkits... ", end='', flush=True)

    degreeFit = getNormalizedDegreeFit(normalizedDegree, 0.3, 0.5)
    visibilityFit = [[(1 - abs(0.5 - visibilityMatrix[x][y])) for y in range(len(visibilityMatrix[0]))] for x in range(len(visibilityMatrix))]

    for i in range(medkit[1]):
        bestTile = getBestTile(roomGraph, diameter, diagonal, medkit, [spawnPoint[0], medkit[0]], placedObjects, degreeFit, visibilityFit, [1, 0.25, 0], [1, 0.25, 0.5])
        addResource(bestTile[0], bestTile[1], medkit[0], roomGraph, map)
        placedObjects.append([bestTile[0], bestTile[1], medkit[0]])

    print("Done.")

    # Place the ammo.
    print("Placing the ammo... ", end='', flush=True)

    degreeFit = getNormalizedDegreeFit(normalizedDegree, 0.2, 0.4)
    visibilityFit = visibilityMatrix

    for i in range(math.floor(ammo[1] / 2)):
        bestTile = getBestTile(roomGraph, diameter, diagonal, ammo, [ammo[0], medkit[0]], placedObjects, degreeFit, visibilityFit, [1, 0.25, 0], [1, 0.25, 0.5])
        addResource(bestTile[0], bestTile[1], ammo[0], roomGraph, map)
        placedObjects.append([bestTile[0], bestTile[1], ammo[0]])

    degreeFit = getNormalizedDegreeFit(normalizedDegree, 0.8, 0.9)

    for i in range(math.ceil(ammo[1] / 2)):
        bestTile = getBestTile(roomGraph, diameter, diagonal, ammo, [ammo[0], medkit[0]], placedObjects, degreeFit, visibilityFit, [1, 0.25, 0], [1, 0.25, 0.5])
        addResource(bestTile[0], bestTile[1], ammo[0], roomGraph, map)
        placedObjects.append([bestTile[0], bestTile[1], ammo[0]])

    print("Done.")
 
# Adds spawn points in safe locations.
def addSpawnPointsSafe(map, rooms, spawnPoint):
    width = len(map)
    height = len(map[0])

    # Removing the objects.
    print("\nRemoving the pre-existing objects... ", end='', flush=True)
    for x in range(width): 
        for y in range(height): 
            if not map[x][y] == "w" and not map[x][y] == "r" :
                map[x][y] = "r"
    print("Done.")

    print("Initializing the variables... ", end='', flush=True)

    roomGraph = getRoomsCorridorsGraph(rooms, False)
    diameter = getDiameterLength(roomGraph)
    diagonal = math.sqrt(math.pow(width, 2) + math.pow(height, 2))

    visibilityMatrix = getVisibilityMatrix(map)
    normalizedDegree = getNormalizedDegree(roomGraph, True)
    placedObjects = []

    print("Done.")

    # Place the spawn points.
    print("Placing the spawn points... ", end='', flush=True)

    degreeFit = dict([(fit[0], 1 - fit[1]) for fit in normalizedDegree])
    visibilityFit = [[(1 - visibilityMatrix[x][y]) for y in range(len(visibilityMatrix[0]))] for x in range(len(visibilityMatrix))]

    for i in range(spawnPoint[1]):
        bestTile = getBestTile(roomGraph, diameter, diagonal, spawnPoint, [spawnPoint[0]], placedObjects, degreeFit, visibilityFit, [1, 0.5, -2], [1, 0.5, 0.5])
        addResource(bestTile[0], bestTile[1], spawnPoint[0], roomGraph, map)
        placedObjects.append([bestTile[0], bestTile[1], spawnPoint[0]])

    print("Done.")

# Adds spawn points in unsafe locations.
def addSpawnPointsUnsafe(map, rooms, spawnPoint):
    width = len(map)
    height = len(map[0])

    # Removing the objects.
    print("\nRemoving the pre-existing objects... ", end='', flush=True)
    for x in range(width): 
        for y in range(height): 
            if not map[x][y] == "w" and not map[x][y] == "r" :
                map[x][y] = "r"
    print("Done.")

    print("Initializing the variables... ", end='', flush=True)

    roomGraph = getRoomsCorridorsGraph(rooms, False)
    diameter = getDiameterLength(roomGraph)
    diagonal = math.sqrt(math.pow(width, 2) + math.pow(height, 2))

    visibilityMatrix = getVisibilityMatrix(map)
    normalizedDegree = getNormalizedDegree(roomGraph, True)
    placedObjects = []
    deadEndCount = 0

    print("Done.")

    # Place the spawn points.
    print("Placing the spawn points... ", end='', flush=True)

    degreeFit = getNormalizedDegreeFit(normalizedDegree, 0.8, 0.9)
    visibilityFit = visibilityMatrix

    for node in list(roomGraph.nodes(data = True)):
        if not roomGraph in normalizedDegree and deadEndCount < spawnPoint[1] / 2:
            candidateTiles = [(x, y, visibilityFit[x][y]) for x in range(node[1]["originX"], node[1]["endX"]) for y in range(node[1]["originY"], node[1]["endY"])]
            bestTile = max(candidateTiles, key = lambda x: x[2])
            addResource(bestTile[0], bestTile[1], spawnPoint[0], roomGraph, map)
            placedObjects.append([bestTile[0], bestTile[1], spawnPoint[0]])
            deadEndCount = deadEndCount + 1

    for i in range(spawnPoint[1] - deadEndCount):
        bestTile = getBestTile(roomGraph, diameter, diagonal, spawnPoint, [spawnPoint[0]], placedObjects, degreeFit, visibilityFit, [1, 1.5, -2], [1, 0.75, 0.75])
        addResource(bestTile[0], bestTile[1], spawnPoint[0], roomGraph, map)
        placedObjects.append([bestTile[0], bestTile[1], spawnPoint[0]])

    print("Done.")

# Adds spawn points in a random uniform way.
def addSpawnPointsUniformly(map, rooms, spawnPoint):
    width = len(map)
    height = len(map[0])

    # Removing the objects.
    print("\nRemoving the pre-existing objects... ", end='', flush=True)
    for x in range(width): 
        for y in range(height): 
            if not map[x][y] == "w" and not map[x][y] == "r" :
                map[x][y] = "r"
    print("Done.")

    print("Initializing the variables... ", end='', flush=True)

    roomGraph = getRoomsCorridorsGraph(rooms, False)
    diagonal = math.sqrt(math.pow(width, 2) + math.pow(height, 2))

    visibilityMatrix = getVisibilityMatrix(map)
    placedObjects = []

    print("Done.")

    # Place the spawn points.
    print("Placing the spawn points... ", end='', flush=True)

    visibilityFit = [[(1 - visibilityMatrix[x][y]) for y in range(len(visibilityMatrix[0]))] for x in range(len(visibilityMatrix))]
    
    for i in range(spawnPoint[1]):
        if (len(placedObjects) > 0):
            bestRoom = getMostIsolatedNode(roomGraph, spawnPoint[0])
        else:
            bestRoom = roomGraph.node[random.choice(list(roomGraph.nodes))]

        print("Done.")
        
        candidateTiles = [(x, y, tileFit(x, y, visibilityFit[x][y], bestRoom["originX"], bestRoom["originY"], bestRoom["endX"], bestRoom["endY"], \
        placedObjects, diagonal, [1, 0.5, 0.5])) for x in range(bestRoom["originX"], bestRoom["endX"]) for y in range(bestRoom["originY"], bestRoom["endY"])]
        bestTile = max(candidateTiles, key = lambda x: x[2])
        addResource(bestTile[0], bestTile[1], spawnPoint[0], roomGraph, map)
        placedObjects.append([bestTile[0], bestTile[1], spawnPoint[0]])        

    print("Done.")

# Adds spawn points in random locations.
def addSpawnPointsRandom(map, rooms, spawnPoint):
    width = len(map)
    height = len(map[0])

    # Removing the objects.
    print("\nRemoving the pre-existing objects... ", end='', flush=True)
    for x in range(width): 
        for y in range(height): 
            if not map[x][y] == "w" and not map[x][y] == "r" :
                map[x][y] = "r"
    print("Done.")

    print("Initializing the variables... ", end='', flush=True)

    roomGraph = getRoomsCorridorsGraph(rooms, False)

    print("Done.")

    # Place the spawn points.
    print("Placing the spawn points... ", end='', flush=True)

    for i in range(spawnPoint[1]):
        room = roomGraph.node[random.choice(list(roomGraph.nodes()))]
        tile = [random.randint(room["originX"], room["endX"]), random.randint(room["originY"], room["endY"])]
        addResource(tile[0], tile[1], spawnPoint[0], roomGraph, map)

    print("Done.")

# Adds a resource to the map.
def addResource(x, y, resource, roomGraph, map):
    width = len(map)

    roomGraph.add_node(subToInd(width, x, y), x = x, y = y, resource = resource)

    for node in roomGraph.nodes(data=True):
        if "originX" in node[1] and x >= node[1]["originX"] and x <= node[1]["endX"] and y >= node[1]["originY"] and y <= node[1]["endY"]:
            roomGraph.add_edge(node[0], subToInd(width, x, y), weight = eulerianDistance(node[1]["originX"] / 2 + node[1]["endX"] / 2, node[1]["originY"] / 2 + node[1]["endY"] / 2, x, y))

    map[x][y] = resource

# Returns the maximum and the minimum visibility.
def minMaxVisibility(G):
    min = math.inf
    max = 0

    for node in G.nodes(data = True):
        if node[1]["visibility"] > max:
            max = node[1]["visibility"]
        elif node[1]["visibility"] < min:
            min = node[1]["visibility"] 

    return min, max    

# Computes the diameter length.
def getDiameterLength(roomGraph):
    shortestPaths = nx.shortest_path_length(roomGraph, None, None, "weight")
    return max([(max(paths[1].values())) for paths in shortestPaths])

# Computes how much each node degree fits the specified interval.
def getDegreeFit(roomGraph, minimum, maximum):
    fitness = [(deg[0], intervalDistance(minimum, maximum, deg[1])) for deg in roomGraph.degree]
    minFit = min(fitness, key = lambda x: x[1])[1]
    maxFit = max(fitness, key = lambda x: x[1])[1]
    return dict([(fit[0], 1 - (fit[1] - minFit) / (maxFit - minFit)) for fit in fitness])

# Computes the normalized degree.
def getNormalizedDegree(roomGraph, discardDeadEnds=False):
    degree = roomGraph.degree
    minDeg = min(degree, key = lambda x: x[1])[1]
    maxDeg = max(degree, key = lambda x: x[1])[1]
    return [(deg[0], (deg[1] - minDeg) / (maxDeg - minDeg)) for deg in roomGraph.degree if (discardDeadEnds is False or deg[1] > 1)]

# Computes how much each normalized degree fits the specified interval.
def getNormalizedDegreeFit(normalizedDegree, minimum, maximum):
    fitness = [(deg[0], intervalDistance(minimum, maximum, deg[1])) for deg in normalizedDegree]
    minFit = min(fitness, key = lambda x: x[1])[1]
    maxFit = max(fitness, key = lambda x: x[1])[1]
    return dict([(fit[0], 1 - (fit[1] - minFit) / (maxFit - minFit)) for fit in fitness])

# Computes a matrix where each cell is the visibility of that cell in the map
# with respect to the visibility of the other cells.
def getVisibilityMatrix(map):
    width = len(map)
    height = len(map[0])

    min = math.inf
    max = 0

    visibilityMap = [[0 for y in range(height)] for x in range(width)] 
     
    for x1 in range(width):
        for y1 in range(height):
            if not map[x1][y1] == "w":
                visibility = 0
                for x2 in range(x1, width):
                    for y2 in range(height):
                        if not map[x2][y2] == "w" and (not x1 == x2 or not y1 == y2) and isTileVisible(x1, y1, x2, y2, map):
                            visibility = visibility + 1
                visibilityMap[x1][y1] = visibility
                if visibility > max:
                    max = visibility
                elif visibility < min:
                    min = visibility
    
    reboundMax = max - min

    for x in range(width):
        for y in range(height):
            visibilityMap[x][y] = (visibilityMap[x][y] - min) / reboundMax

    return visibilityMap
    
# Returns the distance of the closest room to the specified node which contains
# one of the specified resources.
def resourceDistance(graph, diameter, node, resources):
    return min([(shortestPathLength(graph, node, sNode)) if "resource" in data and data["resource"] in resources \
                     else diameter for sNode, data in graph.nodes(data=True)]) / diameter

# Returns how many resource of a give type are in the neighbourhood of the
# node.
def resourceRedundancy(graph, node, resource):
    redundancy = 0
    for neighbor in graph[node]:
        if "resource" in graph.node[neighbor] and graph.node[neighbor]["resource"] is resource[0]:
            redundancy = redundancy + 1 / resource[1]
    return redundancy

# Returns the fitness of a room.
def roomFit(graph, diameter, node, degreeFit, object, objectList, weigths):
    return weigths[0] * degreeFit + weigths[1] * resourceDistance(graph, diameter, node, objectList) + weigths[2] * resourceRedundancy(graph, node, object)

# Returns the distance of a tile from the walls.
def wallDistace(originX, originY, endX, endY, x, y):
    return (min([abs(originX - x), abs(endX - x)]) + min([abs(originY - y), abs(endY - y)])) / ((endX - originX) / 2 + (endY - originY) / 2)

# Returns the distance of a tile from the closest placed object.
def objectDistance(x, y, placedObjects, diagonal):
    return min([(eulerianDistance(x, y, object[0], object[1])) for object in placedObjects]) / diagonal if len(placedObjects) > 0 else 0

# Returns the fitness of a tile.
def tileFit(x, y, visibility, originX, originY, endX, endY, placedObjects, diagonal, weigths):
    return weigths[0] * visibility + weigths[1] * wallDistace(originX, originY, endX, endY, x, y) + weigths[2] * objectDistance(x, y, placedObjects, diagonal)

# Returns the best tile.
def getBestTile(graph, diameter, diagonal, object, objects, placedObjects, degreeFit, visibilityFit, roomWeigths, tileWeigths):
    candidateRooms = [(node, roomFit(graph, diameter, node, degreeFit[node], object, objects, roomWeigths)) \
                      for node, data in graph.nodes(data = True) if (not "resource" in data and node in degreeFit)]
    bestRoom = graph.node[max(candidateRooms, key = lambda x: x[1])[0]]
    candidateTiles = [(x, y, tileFit(x, y, visibilityFit[x][y], bestRoom["originX"], bestRoom["originY"], bestRoom["endX"], bestRoom["endY"], \
                      placedObjects, diagonal, tileWeigths)) for x in range(bestRoom["originX"], bestRoom["endX"]) for y in range(bestRoom["originY"], bestRoom["endY"])]
    return max(candidateTiles, key = lambda x: x[2])

# Returns the node which has the maximum minimum distance from the resource
# nodes.
def getMostIsolatedNode(graph, resource):
    nodes = [(node1, min([nx.shortest_path_length(graph, node1, node2) for node2, data2 in graph.nodes(data = True) if ("resource" in data2 and data2["resource"] == resource)])) \
                      for node1, data1 in graph.nodes(data = True) if ("resource" not in data1)]
    return graph.node[max(nodes, key = lambda x: x[1])[0]]

### GRAPH FUNCTIONS ###########################################################

# Computes the tile graph.
def getTileGraph(map, verbose=True):
    if verbose:
        print("\nGenerating the graph... ", end='', flush=True)

    G = nx.Graph()
    width = len(map)
    height = len(map[0])

    # Add the nodes.
    for x in range(width): 
        for y in range(height): 
            if not map[x][y] == "w":
                G.add_node(subToInd(width, x, y), x = x, y = y, char = map[x][y])

    # Add the edges.
    for x in range(width): 
        for y in range(height): 
            if subToInd(width, x, y) in G:
                if subToInd(width, x + 1, y) in G:
                    G.add_edge(subToInd(width, x + 1, y), subToInd(width, x, y))
                if subToInd(width, x + 1, y + 1) in G:
                    G.add_edge(subToInd(width, x + 1, y + 1), subToInd(width, x, y))                  
                if subToInd(width, x, y + 1) in G:
                    G.add_edge(subToInd(width, x, y + 1), subToInd(width, x, y))
                if subToInd(width, x - 1, y + 1) in G:
                    G.add_edge(subToInd(width, x - 1, y + 1), subToInd(width, x, y))
    
    if verbose:
        print("Done.\n")
        print("The tiles graph has:")
        print("%i nodes." % (nx.number_of_nodes(G)))
        print("%i edges." % (nx.number_of_edges(G)))
    return G

# Computes the rooms and corridors graph.
def getRoomsCorridorsGraph(rooms, verbose=True):
    if verbose:
        print("\nGenerating the graph... ", end='', flush=True)

    G = nx.Graph()

    for i in range(len(rooms)):
        G.add_node("r" + str(i), originX = rooms[i].originX, originY = rooms[i].originY, endX = rooms[i].endX, endY = rooms[i].endY, isCorridor = rooms[i].isCorridor)
    
    for i in range(len(rooms)):
        for j in range(i, len(rooms)):
            if j != i and not (rooms[i].originX >= rooms[j].endX + 1 or rooms[j].originX >= rooms[i].endX + 1) and \
               not (rooms[i].originY >= rooms[j].endY + 1 or rooms[j].originY >= rooms[i].endY + 1):
                G.add_edge("r" + str(i), "r" + str(j), weight = eulerianDistance((rooms[i].originX / 2 + rooms[i].endX / 2), (rooms[i].originY / 2 + rooms[i].endY / 2), (rooms[j].originX / 2 + rooms[j].endX / 2), \
                                                            (rooms[j].originY / 2 + rooms[j].endY / 2)))

    if verbose:
        print("Done.\n")
        print("The rooms and corridor graph has:")
        print("%i nodes." % (nx.number_of_nodes(G)))
        print("%i edges." % (nx.number_of_edges(G)))
    return G

# Computes the rooms, corridors and objects graph.
def getRoomsCorridorsObjectsGraph(rooms, map, verbose=True):
    if verbose:
        print("\nGenerating the graph... ", end='', flush=True)

    G = getRoomsCorridorsGraph(rooms, False)

    width = len(map)
    height = len(map[0])

    for x in range(width): 
        for y in range(height): 
            if not map[x][y] == "w" and not map[x][y] == "r" and not map[x][y] == "d":
                G.add_node(subToInd(width, x, y), x = x, y = y, resource = map[x][y])
                for node in G.nodes(data=True):
                    if "originX" in node[1] and x >= node[1]["originX"] and x <= node[1]["endX"] and y >= node[1]["originY"] and y <= node[1]["endY"]:
                        G.add_edge(node[0], subToInd(width, x, y), weight = eulerianDistance(node[1]["originX"] / 2 + node[1]["endX"] / 2, node[1]["originY"] / 2 + node[1]["endY"] / 2, \
                                                           x, y))
    if verbose:
        print("Done.\n")
        print("The rooms, corridors and objects graph has:")
        print("%i nodes." % (nx.number_of_nodes(G)))
        print("%i edges." % (nx.number_of_edges(G)))
    return G

# Computes the visibility graph.
def getVisibilityGraph(map, verbose=True):
    if verbose:
        print("\nGenerating the graph... ", end='', flush=True)

    G = nx.Graph()
    width = len(map)
    height = len(map[0])

    # Add the nodes.
    for x in range(width): 
        for y in range(height): 
            if not map[x][y] == "w":
                G.add_node(subToInd(width, x, y), x = x, y = y, char = map[x][y], visibility = 0)
     
    # Add the edges.
    for node1 in G.nodes(data=True):
        for node2 in G.nodes(data=True):
            if node1 is not node2 and isTileVisible(node1[1]['x'], node1[1]['y'], node2[1]['x'], node2[1]['y'], map):
                G.add_edge(node1[0], node2[0])

    for node in G.nodes(data = True):
        node[1]['visibility'] = G.degree(node[0])

    if verbose:
        print("Done.\n")
        print("The tiles graph has:")
        print("%i nodes." % (nx.number_of_nodes(G)))
        print("%i edges." % (nx.number_of_edges(G)))
    return G

# Computes the room outlines graph.
def getRoomsOutlineGraph(rooms):
    print("\nGenerating the graph... ", end='', flush=True)

    G = nx.Graph()
    
    i = 0

    for room in rooms:
        G.add_node(i, x = room.originX, y = room.originY)
        i = i + 1
        G.add_node(i, x = room.endX, y = room.originY)
        G.add_edge(i, i - 1)
        i = i + 1
        G.add_node(i, x = room.endX, y = room.endY)
        G.add_edge(i, i - 1)
        i = i + 1
        G.add_node(i, x = room.originX, y = room.endY)
        G.add_edge(i, i - 1)
        G.add_edge(i, i - 3)
        i = i + 1
        
    print("Done.\n")
    print("The tiles graph has:")
    print("%i nodes." % (nx.number_of_nodes(G)))
    print("%i edges." % (nx.number_of_edges(G)))
    return G

### SUPPORT FUNCTIONS #########################################################

# Tells if a tile is visible from another tile.
def isTileVisible(x1, y1, x2, y2, map):
    dy = (y2 - y1) 
    dx = (x2 - x1)

    if dx == 0:
        for y in range(y1, y2) if y1 < y2 else range(y2, y1):
            if map[x1][y] == 'w':
                return False
    elif dy == 0:
        for x in range(x1, x2) if x1 < x2 else range(x2, x1):
            if map[x][y1] == 'w':
                return False
    else:
        m = dy / dx
        c = y1 - m * x1

        if abs(dx) > abs(dy):
            for x in range(x1, x2) if x1 < x2 else range(x2, x1):
                if map[x][int(c + m * x)] == 'w':
                    return False
        else:
            for y in range(y1, y2) if y1 < y2 else range(y2, y1):
                if map[int(y / m - c / m)][y] == 'w':
                    return False
    
    return True

# Tells if a tile is inside the map bounds.
def isInMapRange(x, y, map):
    if (x < len(map[0]) and y < len(map)):
        return True
    else:
        return False

# Converts from subscript to linear index.
def subToInd(width, rows, cols):
    return rows * width + cols

# Converts from linear to subscript index.
def indToSub(width, ind):
    rows = (ind.astype('int') / width)
    cols = (ind.astype('int') % width)
    return (rows, cols)

# Computes the eulerian distance.
def eulerianDistance(x1, y1, x2, y2):
    return math.sqrt(math.pow(x1 - x2, 2) + math.pow(y1 - y2, 2))

# Blends from a value to another.
def blend(a, b, alpha):
  return (1 - alpha) * a + alpha * b

# Coverts from hex to RGB.
def RGBToHex(r, g, b):
    return '#%02x%02x%02x' % (int(r), int(g), int(b))

# Converts from RGB to hex.
def hexToRGB(hex):
    h = hex.lstrip('#')
    RGB = tuple(int(h[i : i + 2], 16) for i in (0, 2 ,4))
    return RGB[0], RGB[1], RGB[2]

# Blends a color.
def blendColor(h1, h2, alpha):
    r1, g1, b1 = hexToRGB(h1)
    r2, g2, b2 = hexToRGB(h2)
    return RGBToHex(blend(r1, r2, alpha), blend(g1, g2, alpha), blend(b1, b2, alpha))

# Tells how well a value fits in an interval.
def intervalDistance(min, max, value):
    #return abs(abs(min) - abs(value)) + abs(abs(max) - abs(value))
    if (value >= min and value <= max):
        return 0
    elif (value < min):
        return abs(min - value)
    else:
        return abs(value - max)

# Computes the shortest path length between two nodes menaging the exception.
def shortestPathLength(graph, n1, n2):
    try: 
        return nx.shortest_path_length(graph, n1, n2, "weight")
    except:
        return 0

# Clears the terminal.
def cls():
    os.system('cls' if os.name == 'nt' else 'clear')

### PLOT FUNCTIONS ############################################################

# Plots the graph.
def plotRoomsCorridorsGraph(G):
    print("\n[CLOSE THE GRAPH TO CONTNUE]")
    pos = dict([(node, (data["originX"] / 2 + data["endX"] / 2, data["originY"] / 2 + data["endY"] / 2)) for node, data in G.nodes(data=True)])
    # edge_labels = dict([(key, "{:.2f}".format(value)) for key, value in
    # nx.get_edge_attributes(G,'weight').items()])
    node_labels = dict([(node, node) for node in G.nodes(data = False)])
    nx.draw_networkx_labels(G, pos, labels = node_labels)
    nx.draw(G, pos, node_color = '#f44242', node_size = 75, node_shape = ",")
    # nx.draw_networkx_edge_labels(G, pos, edge_labels = edge_labels)
    plt.axis('equal')
    plt.savefig(outputDir + "/plot_rooms_graph", dpi = 400)
    plt.show()

# Plots the graph.
def plotRoomsCorridorsObjectsGraph(G):
    print("\n[CLOSE THE GRAPH TO CONTNUE]")
    pos = dict([(node, (data["originX"] / 2 + data["endX"] / 2, data["originY"] / 2 + data["endY"] / 2) if "originX" in data else (data["x"], data["y"])) for node, data  in G.nodes(data=True)])
    # edge_labels = dict([(key, "{:.2f}".format(value)) for key, value in
    # nx.get_edge_attributes(G,'weight').items()])
    colors = [('#f44242' if "originX" in data else "#0079a2") for node, data  in G.nodes(data=True)]
    node_labels = dict([(node, node) if "resource" not in data else (node, data["resource"]) for node, data in G.nodes(data = True)])
    nx.draw_networkx_labels(G, pos, labels = node_labels)
    nx.draw(G, pos, node_color = colors, node_size = 75, node_shape = ",")
    # nx.draw_networkx_edge_labels(G, pos, edge_labels = edge_labels)
    plt.axis('equal')
    plt.savefig(outputDir + "/plot_objects_graph", dpi = 400)
    plt.show()

# Plots the graph.
def plotTilesGraph(G):
    print("\n[CLOSE THE GRAPH TO CONTNUE]")
    pos = dict([ (node, (data["x"], data["y"])) for node, data  in G.nodes(data=True)])
    node_labels = dict([(node, data["char"]) for node, data in G.nodes(data = True) if data["char"] != "w" and data["char"] != "r"])
    nx.draw_networkx_labels(G, pos, labels = node_labels)
    nx.draw(G, pos, node_color = '#f44242', node_size = 75, node_shape = ",")
    plt.axis('equal')
    plt.savefig(outputDir + "/plot_tiles_graph", dpi = 400)
    plt.show()

# Plots the graph.
def plotVisibilityGraph(G):
    print("\n[CLOSE THE GRAPH TO CONTNUE]")
    minC, maxC = minMaxVisibility(G)
    colors = [(blendColor("#0000ff", "#ff0000", (data["visibility"] - minC) / (maxC - minC))) for node, data in G.nodes(data=True)]
    pos = dict([ (node, (data["x"], data["y"])) for node, data in G.nodes(data=True)])
    nx.draw_networkx_nodes(G, pos, node_color = colors, node_size = 75, node_shape = ",")
    # node_labels = nx.get_node_attributes(G,'visibility')
    # nx.draw_networkx_labels(G, pos, labels = node_labels)
    plt.axis('equal')
    plt.savefig(outputDir + "/plot_visibility_graph", dpi = 400)
    plt.show()

# Plots the graph.
def plotOutlinesGraph(G):
    print("\n[CLOSE THE GRAPH TO CONTNUE]")
    nx.draw(G, dict([ (node, (data["x"], data["y"])) for node, data  in G.nodes(data=True)]), node_color = '#f44242', node_size = 75, node_shape = ",")
    plt.axis('equal')
    plt.savefig(outputDir + "/plot_outlines_graph", dpi = 400)
    plt.show()

### MENU FUNCTIONS ############################################################

# Manages the graph menu.
def graphMenu():
    while True:
        print("\n[GRAPH GENERATION] Select an option:")
        print("[1] Generate reachability graph")
        print("[2] Generate visibility graph")
        print("[3] Generate outlines graph")
        print("[0] Back\n")

        quit = False
        option = input("Option: ")

        while option != "1" and option != "2" and option != "3" and option != "0":
            option = input("Invalid choice. Option: ")
        if option == "1":
            graphMenuReachability()
        elif option == "2":
            G = getVisibilityGraph(map)
            plotVisibilityGraph(G)
        elif option == "3":
            G = getRoomsOutlineGraph(rooms)
            plotOutlinesGraph(G)
        elif option == "0":
            return

# Manages the reachability graph menu.
def graphMenuReachability():
    while True:
        print("\n[REACHABILITY GRAPH GENERATION] Select an option:")
        print("[1] Generate tiles graph")
        print("[2] Generate rooms and corridors graph")
        print("[3] Generate rooms, corridors and objects graph")
        print("[0] Back\n")

        option = input("Option: ")
    
        while option != "1" and option != "2" and option != "3" and option != "0":
            option = input("Invalid choice. Option: ")
    
        if option == "1":
            G = getTileGraph(map)
            plotTilesGraph(G)
        elif option == "2":
            G = getRoomsCorridorsGraph(rooms)
            plotRoomsCorridorsGraph(G)
        elif option == "3":
            G = getRoomsCorridorsObjectsGraph(rooms, map)
            plotRoomsCorridorsObjectsGraph(G)
        elif option == "0":
            return

# Menages the file menu.
def filesMenu():
    # Get the name of the map and get the files path.
    mapName, mapFileName, ABFileName, mapFilePath, ABFilePath = getFiles(inputDir)

    # Read the map.
    map = readMap(mapFilePath)

    # Read the AB file.
    rooms = readAB(ABFilePath)
    print("Refining the AB rooms... ", end='', flush=True)
    mergeRooms(rooms)
    rooms = removeRooms(rooms)
    print("Done.")

    return mapName, mapFileName, ABFileName, mapFilePath, ABFilePath, map, rooms

# Mengaes the map population menu.
def populateMenu():
    index = 0

    while True:
        print("\n[MAP POPULATION] Select an option:")
        print("[1] Add spawn points with low risk heuristic")
        print("[2] Add spawn points with high risk heuristic")
        print("[3] Add spawn points uniformly")
        print("[4] Add spawn points randomly")
        print("[5] Add everything")
        print("[0] Back\n")

        option = input("Option: ")
    
        while option != "1" and option != "2" and option != "3" and option != "4" and option != "0":
            option = input("Invalid choice. Option: ")
    
        if option == "1":
            addSpawnPointsSafe(map, rooms, ["s", 5])
            exportMap(outputDir + "/" + mapName + "_SS.map.txt")    
        elif option == "2":
            addSpawnPointsUnsafe(map, rooms, ["s", 5])
            exportMap(outputDir + "/" + mapName + "_SU.map.txt")    
        elif option == "3":
            index = index + 1
            addSpawnPointsUniformly(map, rooms, ["s", 5])
            exportMap(outputDir + "/" + mapName + "_SUD" + str(index) + ".map.txt")         
        elif option == "4":
            addSpawnPointsRandom(map, rooms, ["s", 5])
            exportMap(outputDir + "/" + mapName + "_SR.map.txt")  
        elif option == "5":
            addEverything(map, rooms, ["s", 5], ["h", 4], ["a", 4])
            exportMap(outputDir + "/" + mapName + "_ES.map.txt")  
        elif option == "0":
            return

### MAIN ######################################################################

# Create the input and the output folder if needed.
inputDir = "./Input"
outputDir = "./Output"
if not os.path.exists(inputDir):
    os.makedirs(inputDir)
if not os.path.exists(outputDir):
    os.makedirs(outputDir)

print("MAP ANALYZER\n")
print("This script expects a MAPNAME.map.txt file and MAPNAME_AB.txt file in the input folder.")

# Get the files and process them.
mapName, mapFileName, ABFileName, mapFilePath, ABFilePath, map, rooms = filesMenu()

while True:
    print("\n[MENU] Select an option:")
    print("[1] Populate map")
    print("[2] Generate graphs")
    print("[3] Change files")
    print("[0] Quit\n")

    option = input("Option: ")

    while option != "1" and option != "2" and option != "3" and option != "0":
        option = input("Invalid choice. Option: ")

    if option == "1":
        populateMenu()
    elif option == "2":
        graphMenu()
    elif option == "3":
        mapName, mapFileName, ABFileName, mapFilePath, ABFilePath, map, rooms = filesMenu()
    elif option == "0":
        break