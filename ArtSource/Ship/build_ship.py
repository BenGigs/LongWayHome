"""Editable, metre-scale design study. Run with Blender --background --python.
This generates a new review model; it does not modify the Unity testbed.
"""
import bpy
import math
import os
from mathutils import Vector, Quaternion

ROOT = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(ROOT, 'LongWayHome_Expedition_v03.blend')
if os.path.exists(OUT) and '--replace-generated' not in __import__('sys').argv:
    raise RuntimeError('Review model exists. Save edits separately before explicitly regenerating.')
bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = '01 CUTAWAY — interior review'
scene.unit_settings.system = 'METRIC'
scene.unit_settings.scale_length = 1

def collection(name):
    c = bpy.data.collections.new(name)
    scene.collection.children.link(c)
    return c

shell_back = collection('01 Hull — rear structure')
shell_front = collection('02 Hull — front + cap [hidden in cutaway]')
drive = collection('03 Drive + tanks [concept only]')
work = collection('04 LOWER — workshop + robot lock')
hab = collection('05 MIDDLE — habitat + greenhouse')
cockpit = collection('06 UPPER — flight + science')
access = collection('07 Central stairs + zero-g shaft')
details = collection('08 Exterior radiators + markings')
refs = collection('09 Human scale + deck labels')
studio = collection('10 Review cameras + lighting')

def material(name, color, metal=0, rough=.5, emission=0):
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    p = m.node_tree.nodes.get('Principled BSDF')
    p.inputs['Base Color'].default_value = (*color, 1)
    p.inputs['Metallic'].default_value = metal
    p.inputs['Roughness'].default_value = rough
    if emission:
        p.inputs['Emission Color'].default_value = (*color,1)
        p.inputs['Emission Strength'].default_value = emission
    return m

ivory = material('Hull | ceramic ivory',(.68,.72,.70),.35)
dark = material('Structure | graphite',(.045,.065,.075),.65)
floor = material('Deck | warm grey',(.27,.30,.29),.25)
sage = material('Cabin | sage fabric',(.23,.36,.29))
orange = material('Grips | burnt orange',(.8,.27,.065),.15)
wood = material('Cabin | warm composite',(.38,.20,.10))
green = material('Crops | foliage',(.13,.40,.15))
glass = material('Windows | blue black',(.045,.13,.19),.7,.19)
screen = material('Displays | ice blue',(.12,.6,.7),.2,.35,1.2)
lamp = material('Lighting | warm white',(1,.68,.34),0,.35,3)
white = material('Lettering',(.88,.92,.90))

def link(obj,name,col,mat):
    obj.name=name
    for c in list(obj.users_collection): c.objects.unlink(obj)
    col.objects.link(obj)
    if mat: obj.data.materials.append(mat)
    return obj

def bevel(obj, amount=.04):
    mod=obj.modifiers.new('Soft manufactured edges','BEVEL')
    mod.width=amount; mod.segments=2
    obj.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
    return obj

def box(name,loc,scale,col,mat,angle=0,soft=True):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=link(bpy.context.object,name,col,mat)
    o.dimensions=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    o.rotation_euler.z=angle
    if soft: bevel(o)
    return o

def cylinder(name,loc,r,depth,col,mat,vertices=48):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=depth,location=loc)
    return bevel(link(bpy.context.object,name,col,mat),.025)

def cone(name,z,r1,r2,height,col,mat):
    bpy.ops.mesh.primitive_cone_add(vertices=48,radius1=r1,radius2=r2,depth=height,location=(0,0,z))
    return bevel(link(bpy.context.object,name,col,mat))

def line(name,points,r,col,mat):
    curve=bpy.data.curves.new(name,'CURVE'); curve.dimensions='3D'
    curve.bevel_depth=r; curve.bevel_resolution=2
    p=curve.splines.new('POLY'); p.points.add(len(points)-1)
    for target,co in zip(p.points,points): target.co=(*co,1)
    o=bpy.data.objects.new(name,curve); col.objects.link(o); curve.materials.append(mat)
    return o

def sector(name,inner,outer,z,thickness,start,end,col,mat,steps=24):
    verts=[]
    for dz in [-thickness/2,thickness/2]:
        for r in [inner,outer]:
            for i in range(steps+1):
                a=start+(end-start)*i/steps
                verts.append((r*math.cos(a),r*math.sin(a),z+dz))
    n=steps+1; faces=[]
    for i in range(steps):
        faces += [(i,i+1,n+i+1,n+i),(2*n+i,3*n+i,3*n+i+1,2*n+i+1),
                  (i,2*n+i,2*n+i+1,i+1),(n+i,n+i+1,3*n+i+1,3*n+i)]
    faces += [(0,n,3*n,2*n),(n-1,3*n-1,4*n-1,2*n-1)]
    mesh=bpy.data.meshes.new(name); mesh.from_pydata(verts,[],faces); mesh.update()
    o=bpy.data.objects.new(name,mesh);col.objects.link(o);mesh.materials.append(mat)
    return o

def text(name,body,loc,size,col,mat):
    curve=bpy.data.curves.new(name,'FONT');curve.body=body;curve.size=size
    curve.extrude=.001;curve.align_x='CENTER'
    o=bpy.data.objects.new(name,curve);col.objects.link(o);o.location=loc
    o.rotation_euler=(math.pi/2,0,0);curve.materials.append(mat)
    return o

# 24 m total length; the 10 m cabin permits a 1.2 m stair around a 2 m shaft.
cone('Drive bell | outer shroud',1.8,2.6,1.4,3.6,drive,dark)
cone('Drive throat | inset',.10,2.25,2.15,.2,drive,orange)
cone('Engine service trunk',4.2,1.4,2.4,1.2,drive,dark)
cone('Tank section fairing',7.35,3.1,4.7,5.1,drive,ivory)
for z in [5.2,7.1,9.1]: cylinder('Tank reinforcing band',(0,0,z),3.2+(z-5.2)*.30,.14,drive,dark)
for i in range(8):
    a=i*math.tau/8
    line('External service conduit',[(3.3*math.cos(a),3.3*math.sin(a),5.2),(4.65*math.cos(a),4.65*math.sin(a),9.7)],.055,drive,orange)

levels=[(10,work,'01  WORKSHOP'),(13.4,hab,'02  HABITAT'),(16.8,cockpit,'03  FLIGHT / SCIENCE')]
for z,col,label in levels:
    sector('Deck | rear semicircle',2.25,4.8,z,.18,0,math.pi,col,floor)
    sector('Deck | front semicircle',2.25,4.8,z,.18,math.pi,math.tau,shell_front,floor)
    text(label,label,(0,-4.84,z+.18),.33,refs,white)
    for a in [0,math.pi/4,math.pi/2,3*math.pi/4,math.pi]:
        x,y=4.65*math.cos(a),4.65*math.sin(a)
        box('Cabin rib',(x,y,z+1.6),(.12,.12,3.1),shell_back,ivory)
    # Perimeter grip and warm light strips are individually editable curves.
    for start in [0,.8,1.6,2.4]:
        pts=[(4.52*math.cos(start+t*.65/12),4.52*math.sin(start+t*.65/12),z+1.15) for t in range(13)]
        line('Perimeter handrail',pts,.045,col,orange)
        line('Indirect ceiling strip',[(x,y,z+3.03) for x,y,_ in pts],.035,col,lamp)

# Faceted pressure-shell panels, split into front/rear collections for cutaway.
for start,end,col in [(0,math.pi,shell_back),(math.pi,math.tau,shell_front)]:
    sector('Continuous inner hull',4.77,4.84,13.9,7.8,start,end,col,dark)
    sector('Continuous window band',4.78,4.84,18.65,1.7,start,end,col,glass)
    sector('Upper hull liner',4.77,4.84,19.75,.5,start,end,col,dark)
for i in range(24):
    a=i*math.tau/24; end=(i+1)*math.tau/24
    col=shell_back if math.sin((a+end)/2)>=0 else shell_front
    for z,h in [(11.6,3.2),(15,3.2),(17.35,1.1),(19.75,.5)]:
        sector('Pressure shell panel',4.85,5,z,h,a+.012,end-.012,col,ivory,2)
    sector('Cockpit observation glazing',4.87,4.94,18.65,1.45,a+.022,end-.022,col,glass,2)
    sector('Window mullion',4.83,5.02,18.65,1.5,a-.012,a+.012,col,dark,1)
cone('Protective cap | removable for review',21.75,5,2.4,3.5,shell_front,ivory)
cone('Sensor crown',23.6,2.4,1.5,.4,shell_front,dark)
cylinder('Sensor aperture',(0,0,23.9),.9,.2,shell_front,glass)

# Central spiral stair: 20 x 170 mm risers per deck, clear outer radius 2.2 m.
for level in [10,13.4]:
    for i in range(20):
        a=-math.pi/2+i*math.tau/20
        sector('Stair tread %02d'%i,1.0,2.2,level+.17*(i+1),.12,a,a+math.tau/20,access,floor,3)
    for r in [1.0,2.2]:
        pts=[]
        for i in range(101):
            a=-math.pi/2+i*math.tau/100
            pts.append((r*math.cos(a),r*math.sin(a),level+1.1+3.4*i/100))
        line('Stair continuous grip',pts,.04,access,orange)
        for i in range(0,20,2):
            a=-math.pi/2+i*math.tau/20
            line('Stair guard post',[(r*math.cos(a),r*math.sin(a),level+.17*i),(r*math.cos(a),r*math.sin(a),level+.17*i+1.1)],.025,access,dark)
for a in [0,math.pi]:
    line('Shaft vertical grip',[(.88*math.cos(a),.88*math.sin(a),10),(.88*math.cos(a),.88*math.sin(a),19.6)],.05,access,orange)

# Workshop: rear workbench, module cabinets, and sealed robot transfer volume.
box('Repair bench',(0,3.7,10.85),(2.6,.85,.16),work,wood)
for x in [-1,1]: box('Bench pedestal',(x,3.7,10.42),(.5,.72,.8),work,dark)
box('Tool board',(0,4.3,11.7),(2.5,.10,1.3),work,dark)
for x in [-.8,-.4,0,.4,.8]: box('Tool silhouette',(x,4.20,11.75),(.09,.08,.55),work,orange)
box('Module cabinet',(-3.5,1.4,11.05),(1.05,1.8,2.1),work,ivory)
for z in [10.35,10.85,11.35,11.85]: box('Cabinet drawer',(-3.5,.46,z),(.88,.06,.36),work,sage)
box('Robot transfer lock',(3.4,1.6,11.3),(1.55,2.15,2.6),work,dark)
box('Sealed transfer door',(3.4,.49,11.3),(1.3,.08,2.25),work,ivory)
box('Lock window',(3.4,.42,11.65),(.8,.04,.45),work,glass)
text('Robot label','ROBOT LOCK',(3.4,.36,12.28),.15,work,dark)
box('Robot maintenance cart',(-2.9,-1.65,10.55),(1.15,.8,.9),work,orange)

# Habitat: recognizable scale fixtures, enclosed planter trays and one berth.
box('Sleeping berth',(-3.25,1.6,13.95),(1.4,2.25,.75),hab,wood)
box('Mattress',(-3.25,1.6,14.42),(1.32,2.1,.2),hab,sage)
box('Pillow',(-3.25,2.26,14.58),(1.05,.42,.15),hab,ivory)
box('Personal shelf',(-3.3,2.9,15.18),(1.45,.3,.1),hab,wood)
for i in range(4): box('Book',(-3.7+i*.17,2.88,15.4),(.12,.24,.34),hab,sage if i%2 else orange)
box('Galley',(0,3.9,14),(2.4,.95,1.2),hab,ivory)
box('Galley counter',(0,3.9,14.65),(2.5,1.03,.1),hab,wood)
box('Sink inset',(-.65,3.8,14.72),(.62,.55,.05),hab,dark)
cylinder('Mug',(.62,3.65,14.87),.10,.25,hab,ivory,24)
box('Hygiene enclosure',(-3.35,-1.5,14.5),(1.4,1.4,2.2),hab,sage)
for y in [-.2,1.05,2.3]:
    box('Enclosed hydroponic tray',(3.45,y,14.05),(1.15,1.05,1.1),hab,ivory)
    box('Sealed irrigation lid',(3.45,y,14.65),(1.1,1,.1),hab,dark)
    for dx,dy in [(-.28,-.26),(.28,-.26),(-.28,.26),(.28,.26)]:
        cylinder('Crop stem',(3.45+dx,y+dy,14.95),.025,.55,hab,green,8)
        for j in range(3):
            bpy.ops.mesh.primitive_uv_sphere_add(segments=8,ring_count=4,radius=1,location=(3.45+dx,y+dy,14.82+j*.15))
            leaf=link(bpy.context.object,'Crop foliage',hab,green);leaf.scale=(.19,.14,.085);leaf.rotation_euler.z=j*1.7
    box('Grow light',(3.45,y,15.55),(1.0,.12,.08),hab,lamp)

# Flight deck with a single seat and two distinct science stations.
for x in [-1.15,1.15]:
    box('Flight console',(x,3.25,17.45),(1.8,1.05,1.1),cockpit,dark)
    display=box('Flight display',(x,3.04,18.13),(1.4,.12,.7),cockpit,screen)
    display.rotation_euler.x=math.radians(20)
box('Pilot seat cushion',(-1.15,2.22,17.4),(.8,.8,.2),cockpit,sage)
box('Pilot seat back',(-1.15,1.90,17.88),(.8,.16,1.05),cockpit,sage)
cylinder('Seat pedestal',(-1.15,2.22,17.08),.25,.55,cockpit,dark)
cylinder('Navigation pedestal',(3.25,.4,17.32),.65,.9,cockpit,dark)
cylinder('Navigation surface',(3.25,.4,17.80),.72,.05,cockpit,screen)
for r in [.24,.47]:
    line('Navigation orbit',[(3.25+r*math.cos(a*math.tau/64),.4+r*math.sin(a*math.tau/64),17.88) for a in range(65)],.009,cockpit,white)
box('Science rack',(-3.5,.3,17.8),(1.0,1.6,1.9),cockpit,ivory)
box('Science display',(-3.5,-.53,18.1),(.78,.04,.8),cockpit,screen)

# Radiators are silhouettes only; no thermal engineering is implied.
for sign in [-1,1]:
    box('Radiator spar',(sign*6.5,1.5,7.8),(4,.16,.16),details,dark)
    box('Radiator panel',(sign*7.0,1.5,8.0),(3.8,.13,5.1),details,dark)
    for i in range(9): box('Radiator channel',(sign*7.0,1.40,5.6+i*.6),(3.65,.05,.025),details,ivory,soft=False)
text('Hull designation','L W H  /  0 1',(0,-5.04,11.8),.48,shell_front,dark)
text('Hull mission stripe','EXPEDITION',(0,-5.04,11.25),.18,shell_front,orange)

# 1.8 m astronaut mannequin beside the middle-deck stair landing.
x,y,z=2.7,-1.7,13.49
cylinder('Human torso',(x,y,z+1.12),.23,.66,refs,orange,16)
bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=8,radius=.17,location=(x,y,z+1.63))
link(bpy.context.object,'Human head | total height 1.80 m',refs,ivory)
for side in [-1,1]:
    line('Human leg',[(x+side*.12,y,z+.05),(x+side*.12,y,z+.82)],.095,refs,dark)
    line('Human arm',[(x+side*.31,y,z+.7),(x+side*.29,y,z+1.37)],.075,refs,ivory)

def camera(name,pos,target,ortho):
    data=bpy.data.cameras.new(name);obj=bpy.data.objects.new(name,data);studio.objects.link(obj)
    obj.location=pos;obj.rotation_euler=(Vector(target)-obj.location).to_track_quat('-Z','Y').to_euler()
    data.type='ORTHO';data.ortho_scale=ortho;data.lens=50
    return obj

cutcam=camera('Camera | cutaway',(26,-39,26),(0,0,12),29)
fullcam=camera('Camera | exterior',(30,-44,28),(0,0,12),30)
roomcam=camera('Camera | cabin detail',(15,-23,22),(0,0,15),13)
for name,pos,power,size in [('Key',(12,-18,30),4500,12),('Fill',(-15,-8,19),3500,12),('Rim',(4,12,28),5500,10)]:
    data=bpy.data.lights.new(name,'AREA');data.energy=power;data.shape='DISK';data.size=size
    obj=bpy.data.objects.new(name,data);studio.objects.link(obj);obj.location=pos
    obj.rotation_euler=(Vector((0,0,13))-obj.location).to_track_quat('-Z','Y').to_euler()
for z in [11.8,15.2,18.6]:
    data=bpy.data.lights.new('Cabin bounce','AREA');data.energy=180;data.color=(1,.78,.54);data.size=5
    obj=bpy.data.objects.new('Cabin bounce',data);studio.objects.link(obj);obj.location=(0,-2,z+1)
    obj.rotation_euler=(Vector((0,3,z))-obj.location).to_track_quat('-Z','Y').to_euler()

scene.world=bpy.data.worlds.new('Review backdrop');scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.055,.075,.095,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value=.5
scene.render.engine='CYCLES';scene.cycles.samples=32
scene.cycles.use_denoising=True
scene.render.resolution_x=1200;scene.render.resolution_y=1400;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.view_settings.view_transform='AgX'
scene.camera=cutcam
scene.view_layers[0].layer_collection.children[shell_front.name].exclude=True
full=scene.copy();full.name='02 EXTERIOR — complete hull'
full.use_fake_user=True  # Keep the alternate scene when saving before it is displayed.
full.view_layers[0].layer_collection.children[shell_front.name].exclude=False
full.camera=fullcam
full.view_layers[0].layer_collection.children[refs.name].exclude=True

notes=bpy.data.texts.new('START HERE — model review')
notes.write('THE LONG WAY HOME / EXPEDITION v03\n\nEditable concept model, not a production or engineering asset.\nUnits: metres, Blender +Z = thrust-up. Unity uses +Y.\nEnvelope: 10 m cabin diameter, 24 m height, approx 18 m radiator span.\nDecks: 10.0 / 13.4 / 16.8 m. Stair width 1.2 m; central shaft diameter 2 m.\n\nScene selector: 01 CUTAWAY or 02 EXTERIOR.\nCollections 04, 05, 06 contain room fixtures; 07 contains circulation.\nCollection 02 is excluded only in CUTAWAY and contains front shell, cap, front deck halves.\nNumpad 0: review camera. Middle mouse: orbit; wheel: zoom; Shift+middle mouse: pan.\nSelect an object, then G to move / R to rotate / S to scale.\nSave revisions under a new filename before running the generator again.\n\nDrive, shield and radiator shapes are speculative placeholders.\nRobot lock and lander packaging, stairs/headroom, rails and clear paths need Unity gameplay validation.\nThe Unity scene is unchanged.\n')
bpy.context.window.scene=scene
bpy.ops.object.select_all(action='DESELECT')
for screen_ui in bpy.data.screens:
    for area in screen_ui.areas:
        if area.type=='VIEW_3D':
            space=area.spaces.active;space.clip_end=1000
            space.shading.type='SOLID';space.shading.color_type='MATERIAL';space.shading.light='STUDIO'
            space.shading.show_cavity=True;space.overlay.show_overlays=False
            space.region_3d.view_distance=36;space.region_3d.view_location=(0,0,12)
            space.region_3d.view_rotation=cutcam.rotation_euler.to_quaternion();space.region_3d.view_perspective='PERSP'
scene['Design status']='v03 editable review; dimensions and layout open to change'
scene['Cabin diameter metres']=10.0;scene['Total height metres']=24.0
bpy.ops.wm.save_as_mainfile(filepath=OUT,compress=True)
for active,cam,name in [(scene,cutcam,'cutaway'),(full,fullcam,'exterior'),(scene,roomcam,'interior-detail')]:
    bpy.context.window.scene=active;active.camera=cam
    active.render.filepath=os.path.join(ROOT,'Previews',name+'.png')
    bpy.ops.render.render(write_still=True)
bpy.context.window.scene=scene;scene.camera=cutcam
print('SHIP_MODEL_OK',OUT,'objects',len(bpy.data.objects))
