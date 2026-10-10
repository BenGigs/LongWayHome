"""Editable, metre-scale design study. Run with Blender --background --python.
This generates a new review model; it does not modify the Unity testbed.
"""
import bpy
import math
import os
from mathutils import Vector, Quaternion

ROOT = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(ROOT, 'LongWayHome_Expedition_v04.blend')
if os.path.exists(OUT) and '--replace-generated' not in __import__('sys').argv:
    raise RuntimeError('Review model exists. Save edits separately before explicitly regenerating.')
bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = '01 CUTAWAY — five open decks'
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

# Five-deck replacement layout. The preceding helpers are shared in shape with v03.
work.name='06 Deck 3 — workshop + robot deployment'
hab.name='05 Deck 4 — habitat + water'
cockpit.name='08 Deck 1 — cockpit'
access.name='09 OPEN shaft — pole + grab rings (no barriers)'
refs.name='11 Human scale + labels'
studio.name='12 Review lighting + cameras'
details.name='10 Exterior details'
fuel=collection('04 Deck 5 — fuel + storage')
nav=collection('07 Deck 2 — navigation + open sleeping nook')

# Metres: five 3.2 m deck intervals, 8 m diameter cabin, 25 m complete envelope.
R=3.85
SHAFT_X=0.9
SHAFT_Y=-0.85
HOLE=1.05
POLE_X=SHAFT_X+HOLE-.18
POLE_Y=SHAFT_Y
floors=[6.2,9.4,12.6,15.8,19.0]
levels=[(floors[0],fuel,'05  FUEL / STORAGE'),(floors[1],hab,'04  HABITAT / WATER'),
        (floors[2],work,'03  WORKSHOP / ROBOT'),(floors[3],nav,'02  NAVIGATION / SLEEP'),
        (floors[4],cockpit,'01  COCKPIT')]

def deck(name,z,col,front=False):
    # Annulus triangulated around an offset hatch: intersect radial rays with cabin circle.
    start,end=(-math.pi,0) if front else (0,math.pi)
    count=64; verts=[]
    for dz in [-.09,.09]:
        for outer in [False,True]:
            for i in range(count+1):
                a=start+(end-start)*i/count; dx,dy=math.cos(a),math.sin(a)
                b=SHAFT_X*dx+SHAFT_Y*dy
                radius=-b+math.sqrt(b*b+R*R-SHAFT_X*SHAFT_X-SHAFT_Y*SHAFT_Y) if outer else HOLE
                verts.append((SHAFT_X+radius*dx,SHAFT_Y+radius*dy,z+dz))
    n=count+1;faces=[]
    for i in range(count):
        faces.extend([(i,i+1,n+i+1,n+i),(2*n+i,3*n+i,3*n+i+1,2*n+i+1),
                      (i,2*n+i,2*n+i+1,i+1),(n+i,n+i+1,3*n+i+1,3*n+i)])
    faces.extend([(0,n,3*n,2*n),(n-1,3*n-1,4*n-1,2*n-1)])
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    o=bpy.data.objects.new(name,mesh);col.objects.link(o);mesh.materials.append(floor)

bright=material('Grab targets | luminous amber',(1,.38,.055),.05,.4,1.1)
cyan=material('Water | teal',(.06,.32,.36),.2)
for z,col,label in levels:
    deck(label+' floor rear',z,col)
    deck(label+' floor front [cutaway removes]',z,shell_front,True)
    # Ring is at deck level: no raised hoop, fencing or guardrail across the opening.
    points=[(SHAFT_X+(HOLE+.06)*math.cos(i*math.tau/96),SHAFT_Y+(HOLE+.06)*math.sin(i*math.tau/96),z+.08) for i in range(97)]
    line('DECK GRAB RING | '+label,points,.055,access,orange)
    for i in range(8):
        a=i*math.tau/8
        line('Luminous grab-ring marker',[(SHAFT_X+1.115*math.cos(a+t*.09),SHAFT_Y+1.115*math.sin(a+t*.09),z+.145) for t in range(4)],.016,access,bright)
    text('Deck identity | '+label,label,(0,3.45,z+2.55),.23,col,white)
    for a in [0,math.pi/4,math.pi/2,3*math.pi/4,math.pi]:
        x,y=3.78*math.cos(a),3.78*math.sin(a)
        box('Interior rib',(x,y,z+1.55),(.075,.075,2.95),shell_back,ivory)
    line('Warm room light',[(3.65*math.cos(a*math.pi/40),3.65*math.sin(a*math.pi/40),z+2.93) for a in range(41)],.025,col,lamp)

# Continuous unobstructed drop path on the opposite side of the pole.
cylinder('CLIMB POLE | continuous', (POLE_X,POLE_Y,(floors[0]+floors[-1]+2.8)/2),.065,floors[-1]+2.8-floors[0],access,dark,32)
for i in range(34):
    z=floors[0]+.25+i*.46
    cylinder('Pole orange grip sleeve',(POLE_X,POLE_Y,z),.083,.22,access,orange,24)
    cylinder('Pole luminous band',(POLE_X,POLE_Y,z+.13),.086,.025,access,bright,24)

# Smooth cylindrical pressure hull, front half removable only in the review scene.
for start,end,col in [(0,math.pi,shell_back),(math.pi,math.tau,shell_front)]:
    sector('Pressure liner',3.88,4.0,12.8,13.2,start,end,col,ivory,48)
    sector('Cockpit sill',3.88,4.0,19.45,.5,start,end,col,ivory,48)
    sector('Cockpit wraparound windows',3.9,4.0,20.55,1.7,start,end,col,glass,48)
    sector('Cockpit header',3.88,4.0,21.6,.4,start,end,col,ivory,48)
    for z in floors:
        sector('Exterior deck seam',4.0,4.035,z,.10,start,end,col,dark,48)
    for i in range(12):
        a=start+(end-start)*i/12
        line('Hull service seam',[(4.01*math.cos(a),4.01*math.sin(a),6.2),(4.01*math.cos(a),4.01*math.sin(a),19.7)],.012,col,dark)
        line('Window mullion',[(3.98*math.cos(a),3.98*math.sin(a),19.7),(3.98*math.cos(a),3.98*math.sin(a),21.4)],.045,col,ivory)
cone('Rounded nose base',22.4,4,3.2,1.2,shell_front,ivory)
cone('Nose cap',23.6,3.2,1.5,1.2,shell_front,ivory)
cone('Sensor crown',24.6,1.5,.4,.8,shell_front,dark)
cone('Drive bell',1.45,2.0,1.1,2.9,drive,dark)
cylinder('Throat',(0,0,.1),1.77,.18,drive,orange)
cone('Engineering buffer',4.55,1.1,4,3.3,drive,ivory)
for a in [0,math.pi/2,math.pi,3*math.pi/2]:
    line('Drive conduit',[(1.4*math.cos(a),1.4*math.sin(a),3),(3.75*math.cos(a),3.75*math.sin(a),6)],.07,drive,orange)

# Bottom room: sealed fuel vessels and accessible storage around the clear floor.
z=floors[0]
for x in [-2.5,0,2.5]:
    y=2.05 if x else 2.75
    cylinder('Sealed fuel vessel',(x,y,z+1.25),.52,2.3,fuel,dark)
    for dz in [.38,1.9]: cylinder('Fuel tank restraint band',(x,y,z+dz),.54,.10,fuel,orange)
    text('Fuel tank label','FUEL',(x,y-.56,z+1.35),.15,fuel,white)
for y in [-1.6,-.5,.6]:
    box('Supply locker',(-2.65,y,z+.68),(1.05,.95,1.2),fuel,ivory)
    box('Locker latch',(-2.07,y,z+.85),(.07,.26,.08),fuel,orange)
box('Fuel service panel',(2.75,.3,z+1.05),(.7,.85,1.8),fuel,dark)

# Habitat and water: plants to one side, water treatment opposite, clear central floor.
z=floors[1]
for x in [-1.5,0,1.5]:
    box('Enclosed hydroponic planter',(x,2.3,z+.7),(1.15,1.1,1.1),hab,ivory)
    box('Irrigation lid',(x,2.3,z+1.3),(1.1,1.05,.1),hab,dark)
    for dx in [-.27,.27]:
        for dy in [-.25,.25]:
            cylinder('Plant stem',(x+dx,2.3+dy,z+1.58),.024,.5,hab,green,8)
            for h in [1.45,1.63,1.8]:
                bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=.22,location=(x+dx,2.3+dy,z+h))
                leaf=link(bpy.context.object,'Plant foliage',hab,green);leaf.scale.z=.45
    box('Grow lighting',(x,2.3,z+2.3),(1.0,.18,.065),hab,lamp)
for y in [-.6,.8]: cylinder('Water reservoir',(-2.65,y,z+1.15),.5,2.05,hab,cyan)
box('Water recycling filters',(-2.3,-1.8,z+.85),(1.15,.75,1.6),hab,ivory)
box('Compact food preparation counter',(2.7,1,z+.6),(.95,1.6,1.1),hab,wood)

# Workshop / deployment: open robot recess, no door panels in this design revision.
z=floors[2]
box('Repair workbench',(-1.0,2.5,z+.9),(2.5,.85,.16),work,wood)
for x in [-1.85,-.2]: box('Bench pedestal',(x,2.5,z+.46),(.45,.7,.85),work,dark)
box('Tool board',(-1,3.12,z+1.75),(2.6,.10,1.3),work,dark)
for x in [-1.8,-1.4,-1,-.6,-.2]: box('Visible tool',(x,3.04,z+1.75),(.09,.08,.5),work,orange)
box('Robot deployment cradle',(2.35,1.75,z+.22),(1.5,1.9,.25),work,dark)
box('Robot chassis',(2.35,1.75,z+.65),(1.1,1.15,.55),work,ivory)
for x in [1.75,2.95]:
    for y in [1.4,2.1]:
        wheel=cylinder('Robot wheel',(x,y,z+.46),.25,.18,work,dark,20);wheel.rotation_euler.y=math.pi/2
box('Robot sensor mast',(2.35,1.85,z+1.12),(.2,.2,.6),work,orange)
box('Robot optics',(2.35,1.78,z+1.5),(.45,.25,.22),work,screen)
for y in [-1.6,-.4,.8]: box('Module storage',(-2.7,y,z+.85),(.85,.95,1.6),work,ivory)

# Navigation room with separate open nook. Table does not obstruct the hatch.
z=floors[3]
cylinder('Navigation table pedestal',(-.65,1.25,z+.58),.42,1.0,nav,dark)
cylinder('Navigation table',(-.65,1.25,z+1.12),1.05,.14,nav,wood)
cylinder('Star map surface',(-.65,1.25,z+1.21),.92,.025,nav,screen)
for r in [.3,.55,.8]:
    line('Map orbit',[( -.65+r*math.cos(i*math.tau/64),1.25+r*math.sin(i*math.tau/64),z+1.25) for i in range(65)],.01,nav,white)
box('OPEN sleeping nook | bed',( -2.5,-.55,z+.48),(1.25,2.15,.7),nav,wood)
box('Mattress',(-2.5,-.55,z+.9),(1.2,2.05,.18),nav,sage)
box('Pillow',(-2.5,.12,z+1.03),(1.05,.42,.14),nav,ivory)
box('Personal shelf',(-2.5,.77,z+1.48),(1.35,.25,.1),nav,wood)
for i in range(4): box('Book',(-2.9+.18*i,.77,z+1.67),(.12,.2,.28),nav,orange if i%2 else sage)
box('Navigation storage',(2.5,1.6,z+.72),(1.0,1.45,1.3),nav,ivory)

# Cockpit on top: one seat, front consoles, no navigation table duplication.
z=floors[4]
for x in [-.95,.95]:
    box('Cockpit console',(x,2.4,z+.6),(1.65,.9,1.0),cockpit,dark)
    display=box('Flight display',(x,2.13,z+1.25),(1.3,.1,.6),cockpit,screen);display.rotation_euler.x=.3
box('Pilot seat',(0,1.25,z+.62),(.85,.8,.2),cockpit,sage)
box('Pilot backrest',(0,.94,z+1.03),(.85,.14,.85),cockpit,sage)
cylinder('Seat pedestal',(0,1.25,z+.3),.22,.5,cockpit,dark)
box('Flight equipment rack',(-2.6,0,z+.85),(.9,1.0,1.55),cockpit,ivory)

# Human reference safely clear of the opening, used only for scale judgment.
x,y,z=-.6,-1.1,floors[3]+.09
cylinder('Human torso',(x,y,z+1.1),.22,.65,refs,orange,16)
bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=8,radius=.17,location=(x,y,z+1.63))
link(bpy.context.object,'Human head | 1.8 m reference',refs,ivory)
for side in [-1,1]:
    line('Human leg',[(x+side*.12,y,z+.05),(x+side*.12,y,z+.8)],.085,refs,dark)
    line('Human arm',[(x+side*.29,y,z+.7),(x+side*.28,y,z+1.35)],.07,refs,ivory)
for sign in [-1,1]:
    box('Radiator support',(sign*4.8,1.2,4.9),(3.2,.15,.15),details,dark)
    box('Radiator',(sign*5.5,1.2,4.8),(2.4,.12,4.2),details,dark)
    for i in range(8): box('Radiator channel',(sign*5.5,1.12,3+i*.5),(2.25,.04,.025),details,ivory,soft=False)
text('Ship marking','L W H  /  0 4',(0,-4.05,17.0),.38,shell_front,dark)

def camera(name,pos,target,scale):
    data=bpy.data.cameras.new(name);obj=bpy.data.objects.new(name,data);studio.objects.link(obj)
    obj.location=pos;obj.rotation_euler=(Vector(target)-obj.location).to_track_quat('-Z','Y').to_euler()
    data.type='ORTHO';data.ortho_scale=scale
    return obj

cutcam=camera('Camera | five deck cutaway',(25,-43,26),(0,0,13.4),28)
fullcam=camera('Camera | complete cylinder',(30,-45,28),(0,0,12.5),29)
detailcam=camera('Camera | open shaft and rings',(11,-18,23),(0,0,15),12)
for name,pos,power in [('Key',(12,-18,28),4200),('Fill',(-12,-10,20),3300),('Rim',(4,12,28),5000)]:
    data=bpy.data.lights.new(name,'AREA');data.energy=power;data.shape='DISK';data.size=10
    obj=bpy.data.objects.new(name,data);studio.objects.link(obj);obj.location=pos
    obj.rotation_euler=(Vector((0,0,13))-obj.location).to_track_quat('-Z','Y').to_euler()
for z,_,_ in levels:
    data=bpy.data.lights.new('Warm cabin bounce','AREA');data.energy=130;data.color=(1,.78,.55);data.size=4
    obj=bpy.data.objects.new('Warm cabin bounce',data);studio.objects.link(obj);obj.location=(0,-1.9,z+2.7)
    obj.rotation_euler=(Vector((0,2,z+1))-obj.location).to_track_quat('-Z','Y').to_euler()
scene.world=bpy.data.worlds.new('Review background');scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.055,.075,.095,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value=.5
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True
scene.render.resolution_x=1100;scene.render.resolution_y=1500;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
scene.camera=cutcam
scene.view_layers[0].layer_collection.children[shell_front.name].exclude=True
full=scene.copy();full.name='02 EXTERIOR — five deck cylinder';full.use_fake_user=True
full.view_layers[0].layer_collection.children[shell_front.name].exclude=False
full.view_layers[0].layer_collection.children[refs.name].exclude=True
full.camera=fullcam
notes=bpy.data.texts.new('START HERE — five open decks');notes.use_fake_user=True
notes.write('LWH v04 / FIVE OPEN DECKS\nTop to bottom: cockpit; navigation + open sleeping nook; workshop + robot deployment; habitat + water; fuel + storage.\nNo doors, stairs, guardrails or raised hatch barriers.\n2.1 m aligned hatch openings, offset from cabin centre. Deck-level orange grab rings and a side-mounted climbing pole leave an open falling/coasting path.\nBlender review geometry only: powered pole climbing is NOT implemented in Unity.\nScene selector: CUTAWAY or EXTERIOR. Front floor halves and front hull are hidden only in CUTAWAY. Full model contains complete decks.\n8 m cabin diameter; 25 m tall; 3.2 m deck pitch; units metres / Blender +Z up.\nMiddle mouse orbit; wheel zoom; Shift+middle mouse pan. Select named fixtures, G/R/S to edit. Save revisions under a new filename.\nThe robot deployment station reserves space only; lander packaging and pressure transfer remain future design work.\n')
bpy.context.window.scene=scene
bpy.ops.object.select_all(action='DESELECT')
for s in bpy.data.screens:
    for area in s.areas:
        if area.type=='VIEW_3D':
            sp=area.spaces.active;sp.clip_end=1000;sp.shading.type='SOLID';sp.shading.color_type='MATERIAL'
            sp.shading.show_cavity=True;sp.overlay.show_overlays=False
            sp.region_3d.view_distance=36;sp.region_3d.view_location=(0,0,13)
            sp.region_3d.view_rotation=cutcam.rotation_euler.to_quaternion();sp.region_3d.view_perspective='PERSP'
scene['Design revision']='v04 | 5 rooms, open hatches, no stairs/doors/guardrails'
scene['Hatch diameter metres']=2.1;scene['Climbing implemented in Unity']=False
os.makedirs(os.path.join(ROOT,'Previews','v04'),exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=OUT,compress=True)
for active,cam,name in [(scene,cutcam,'cutaway'),(full,fullcam,'exterior'),(scene,detailcam,'grab-rings')]:
    bpy.context.window.scene=active;active.camera=cam
    active.render.filepath=os.path.join(ROOT,'Previews','v04',name+'.png')
    bpy.ops.render.render(write_still=True)
print('FIVE_DECK_MODEL_OK',OUT,len(bpy.data.objects),flush=True)
