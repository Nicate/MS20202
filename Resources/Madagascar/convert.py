import bpy
import math
import mathutils
import xml
class TransverseMercator:
    radius = 6378137
    def __init__(self, **kwargs):
        # setting default values
        self.lat = 0 # in degrees
        self.lon = 0 # in degrees
        self.k = 1 # scale factor
        for attr in kwargs:
            setattr(self, attr, kwargs[attr])
        self.latInRadians = math.radians(self.lat)
    def fromGeographic(self, lat, lon):
        lat = math.radians(lat)
        lon = math.radians(lon-self.lon)
        B = math.sin(lon) * math.cos(lat)
        x = 0.5 * self.k * self.radius * math.log((1+B)/(1-B))
        y = self.k * self.radius * ( math.atan(math.tan(lat)/math.cos(lon)) - self.latInRadians )
        return (x,y)
    def toGeographic(self, x, y):
        x = x/(self.k * self.radius)
        y = y/(self.k * self.radius)
        D = y + self.latInRadians
        lon = math.atan(math.sinh(x)/math.cos(D))
        lat = math.asin(math.sin(D)/math.cosh(x))
        lon = self.lon + math.degrees(lon)
        lat = math.degrees(lat)
        return (lat, lon)
def convert(lat, lon):
    projection = TransverseMercator(lat=bpy.context.scene["lat"], lon=bpy.context.scene["lon"])
    return mathutils.Vector(projection.fromGeographic(lat, lon)) / 100000
def list(fileName):
    nodes = []
    gpx = xml.etree.cElementTree.parse(fileName).getroot()
    for element in gpx:
        if element.tag[element.tag.find("}")+1:] == "wpt":
            lat = float(element.attrib["lat"])
            lon = float(element.attrib["lon"])
            node = (lat, lon)
            nodes.append(node)
    return nodes
def query(fileName):
    nodes = list(fileName)
    return [convert(lat, lon) for lat, lon in nodes]
def output(fileName):
    for point in query(fileName):
        print(point.x, point.y)
def cat(fileName):
    return "\n".join(str(x) + "," + str(y) for x, y in query(fileName))
def store(fileName, text):
    with open(fileName, "w") as file:
        file.write(text)
def go(fileName):
    return store(fileName[:fileName.find(".")] + ".xy", cat(fileName))
go("C:\\Projects\\Madagascar Simulator 2020\\Resources\\Madagascar\\capital.gpx")