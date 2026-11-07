"""Pruebas reales de caja negra contra ASP.NET Core + Identity (perfil Demo/SQLite).
Uso: pip install requests beautifulsoup4; python verificar_http.py --project RUTA_PROYECTO
Requiere .NET SDK 8; crea una BD temporal aislada. No modifica la BD de la usuaria.
"""
from pathlib import Path
import argparse, os, subprocess, time, tempfile, sqlite3, json, datetime, sys
import requests, html as htmlmod
from bs4 import BeautifulSoup
p=argparse.ArgumentParser();p.add_argument('--project',required=True);p.add_argument('--dotnet',default='dotnet');p.add_argument('--report',default='resultado_http.json');p.add_argument('--port',type=int,default=5192);args=p.parse_args()
Path(args.report).resolve().parent.mkdir(parents=True,exist_ok=True)
project=Path(args.project).resolve();base=f'http://127.0.0.1:{args.port}';results=[]
def check(id,description,condition):
 results.append({'id':id,'prueba':description,'resultado':'APROBADA' if condition else 'FALLÓ'});print(f'{results[-1]["resultado"]}: {id} {description}',flush=True)
 if not condition:raise AssertionError(description)
def get(s,path):
 r=s.get(base+path);r._content=htmlmod.unescape(r.text).encode('utf-8');r.encoding='utf-8';assert r.status_code<500,(path,r.status_code);return r
def post(s,path,data=None,tokenpath=None):
 r=get(s,tokenpath or path);soup=BeautifulSoup(r.text,'html.parser');token=soup.select_one('input[name=__RequestVerificationToken]');assert token,(path,r.url,r.text[:300]);d=dict(data or {});d['__RequestVerificationToken']=token['value'];r=s.post(base+path,data=d,allow_redirects=False);r._content=htmlmod.unescape(r.text).encode('utf-8');r.encoding='utf-8';assert r.status_code<500,(path,r.status_code,r.text[:500]);return r
def login(email,password):
 s=requests.Session();r=post(s,'/Account/Login',{'Correo':email,'Password':password});assert r.status_code==302,(r.status_code,r.text[:100]);return s
def scalar(sql,*params):
  c=sqlite3.connect(dbfile)
  try:return c.execute(sql,params).fetchone()[0]
  finally:c.close()
def execute(sql,*params):
  c=sqlite3.connect(dbfile)
  try:c.execute(sql,params);c.commit()
  finally:c.close()
def record():
 Path(args.report).write_text(json.dumps({'fecha_utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'entorno':'ASP.NET Core MVC / Identity / EF Core / SQLite Demo','resultado_global':'APROBADO' if all(x['resultado']=='APROBADA' for x in results) and len(results)==52 else 'INCOMPLETO','total':len(results),'pruebas':results},ensure_ascii=False,indent=2),encoding='utf-8')
with tempfile.TemporaryDirectory(prefix='campus-test-') as temp:
 dbfile=str(Path(temp)/'pruebas.db');env=os.environ.copy();env.update({'ASPNETCORE_ENVIRONMENT':'Demo','ASPNETCORE_URLS':base,'ConnectionStrings__DemoConnection':f'Data Source={dbfile}','Logging__LogLevel__Default':'Warning'})
 log=open(Path(temp)/'server.log','w+');server=subprocess.Popen([args.dotnet,str(project/'bin/Debug/net8.0/ProyectoFinal_VargasAnaCarolina.dll')],cwd=project,env=env,stdout=log,stderr=subprocess.STDOUT)
 try:
  for _ in range(160):
   try:
    if requests.get(base+'/Account/Login',timeout=1).status_code==200:break
   except requests.RequestException:pass
   if server.poll()!=None:log.seek(0);raise RuntimeError(log.read())
   time.sleep(.25)
  else:raise TimeoutError('No se inició el servidor')
  anon=requests.Session();r=anon.get(base+'/',allow_redirects=False);check('SEG-01','Panel protegido exige autenticación',r.status_code==302 and '/Account/Login' in r.headers['Location'])
  admin=login('admin@campus.local','Campus!2026Demo');check('HU02-1A','Administrador accede a su panel','Panel de administración' in get(admin,'/').text)
  career1=scalar('select Id from Carreras where Codigo=?','IS');career2=scalar('select Id from Carreras where Codigo=?','AE')
  data={'Identificacion':'TEST-001','Nombre':'Prueba Estudiante','Correo':'prueba@campus.local','Telefono':'8888-1111','Direccion':'Heredia','CarreraId':career1,'Password':'Prueba!2026','ConfirmPassword':'Prueba!2026','Role':'Administrador'}
  student=requests.Session();r=post(student,'/Account/Register',data);check('HU01-1','Registro crea cuenta y expediente de forma atómica',r.status_code==302 and scalar('select count(*) from Estudiantes where Identificacion=?','TEST-001')==1)
  sid=scalar('select Id from Estudiantes where Identificacion=?','TEST-001');uid=scalar('select UserId from Estudiantes where Id=?',sid)
  check('HU20-3A','Registro manipulado no permite autoasignar Administrador',scalar('select r.Name from AspNetUserRoles ur join AspNetRoles r on ur.RoleId=r.Id where ur.UserId=?',uid)=='Estudiante')
  check('HU02-1E','Estudiante accede a su panel','Mi portal académico' in get(student,'/').text)
  r=post(anon,'/Account/Register',{**data,'Identificacion':'TEST-002'});check('HU01-3A','Correo duplicado produce error amigable','correo ya está registrado' in r.text and r.status_code==200)
  r=post(anon,'/Account/Register',{**data,'Correo':'otra@campus.local'});check('HU01-3B','Identificación duplicada produce error amigable','identificación ya está registrada' in r.text)
  before=scalar('select count(*) from AspNetUsers');r=post(anon,'/Account/Register',{**data,'Correo':'incompleta@campus.local','Nombre':''});check('HU01-2A','Datos incompletos no generan cuenta parcial',r.status_code==200 and scalar('select count(*) from AspNetUsers')==before)
  r=post(anon,'/Account/Login',{'Correo':'prueba@campus.local','Password':'Incorrecta!'});check('HU02-2','Credenciales inválidas producen mensaje general','No fue posible ingresar' in r.text)
  html=get(student,'/Perfil').text;check('HU03-1','Perfil muestra solo el expediente propio','Prueba Estudiante' in html and 'Estudiante de demostración' not in html)
  r=post(student,'/Perfil',{'Telefono':'8777-2222','Direccion':'Alajuela'});check('HU03-2','Contacto válido persiste',r.status_code==302 and scalar('select Direccion from Estudiantes where Id=?',sid)=='Alajuela')
  r=post(student,'/Perfil',{'Telefono':'8777-2222','Direccion':'Forjada','Id':1,'Identificacion':'OTRA','CarreraId':career2,'Role':'Administrador'});check('HU03-3','Manipulación del expediente se rechaza','Solo puede modificar' in r.text and scalar('select Direccion from Estudiantes where Id=?',sid)=='Alajuela' and scalar('select CarreraId from Estudiantes where Id=?',sid)==career1)
  html=get(student,'/Catalogo').text;check('HU04-1','Catálogo contiene código, nombre y descripción','IS' in html and 'Ingeniería en Sistemas' in html and 'Desarrollo de software' in html)
  html=get(anon,f'/Account/Register?carreraId={career2}').text;selected=BeautifulSoup(html,'html.parser').select_one('select[name=CarreraId] option[selected]');check('HU04-2','Carrera elegida se refleja en registro',selected is not None and selected.get('value')==str(career2))
  check('HU04-3','Cambio de carrera exige canal administrativo','revisión' in get(student,'/Perfil').text and 'administración' in get(student,'/Catalogo').text)
  for path in ['/Carreras','/Cursos','/Docentes','/Estudiantes','/Estudiantes/Acceso/'+str(sid)]:
   r=student.get(base+path,allow_redirects=False);check('HU20-3B'+path,'Ruta administrativa bloqueada '+path,r.status_code in (302,403) and (r.status_code==403 or '/Account/Denied' in r.headers['Location']))
  r=admin.post(base+'/Carreras/Create',data={'Codigo':'CSRF','Nombre':'Forjada','Descripcion':'Sin token'},allow_redirects=False);check('SEG-02','POST sin token antifalsificación se rechaza',r.status_code==400 and scalar('select count(*) from Carreras where Codigo=?','CSRF')==0)
  careerdata={'Codigo':'TEST','Nombre':'Carrera de Prueba','Descripcion':'Descripción de prueba'}
  r=post(admin,'/Carreras/Create',careerdata);cid=scalar('select Id from Carreras where Codigo=?','TEST');check('HU12-1','Crear y consultar carrera en catálogo administrativo',r.status_code==302 and 'Carrera de Prueba' in get(admin,'/Carreras').text)
  r=post(admin,f'/Carreras/Edit/{cid}',{**careerdata,'Nombre':'Carrera actualizada'});check('HU12-2','Editar carrera conserva código único',r.status_code==302 and scalar('select Nombre from Carreras where Id=?',cid)=='Carrera actualizada')
  r=post(admin,'/Carreras/Create',careerdata);check('HU12-DUP','Código de carrera duplicado se rechaza','código ya está registrado' in r.text)
  course={'Codigo':'TEST-101','Nombre':'Curso de Prueba','Creditos':'4','CarreraId':cid}
  r=post(admin,'/Cursos/Create',course);courseid=scalar('select Id from Cursos where Codigo=?','TEST-101');check('HU13-1','Crear curso con carrera y créditos válidos',r.status_code==302 and 'Curso de Prueba' in get(admin,'/Cursos').text)
  r=post(admin,f'/Cursos/Edit/{courseid}',{**course,'Nombre':'Curso actualizado','Creditos':'5'});check('HU13-EDIT','Editar curso persiste',r.status_code==302 and scalar('select Creditos from Cursos where Id=?',courseid)==5)
  for credits in ['0','13','3.5']:
   r=post(admin,'/Cursos/Create',{**course,'Codigo':'INVALIDO','Creditos':credits});check('HU13-2-'+credits,'Créditos inválidos rechazados: '+credits,r.status_code==200 and scalar('select count(*) from Cursos where Codigo=?','INVALIDO')==0)
  r=post(admin,'/Cursos/Create',course);check('HU13-DUP','Código de curso duplicado se rechaza','código ya está registrado' in r.text)
  teacher={'Identificacion':'DOC-TEST','Nombre':'Docente de Prueba','Correo':'doc-test@campus.local','Telefono':'8777-1234'}
  r=post(admin,'/Docentes/Create',teacher);tid=scalar('select Id from Docentes where Identificacion=?','DOC-TEST');check('HU14-1','Crear y consultar docente',r.status_code==302 and 'Docente de Prueba' in get(admin,'/Docentes').text)
  r=post(admin,f'/Docentes/Edit/{tid}',{**teacher,'Nombre':'Docente actualizado'});check('HU14-EDIT','Editar docente conserva contacto válido',r.status_code==302 and scalar('select Nombre from Docentes where Id=?',tid)=='Docente actualizado')
  r=post(admin,'/Docentes/Create',teacher);check('HU14-2A','Identificación y correo de docente duplicados se rechazan','identificación ya está registrada' in r.text and 'correo ya está registrado' in r.text)
  r=post(admin,'/Docentes/Create',{**teacher,'Identificacion':'DOC-EMPTY','Correo':'otro-doc@campus.local','Nombre':''});check('HU14-2B','Docente sin nombre se rechaza',r.status_code==200 and scalar('select count(*) from Docentes where Identificacion=?','DOC-EMPTY')==0)
  now=datetime.datetime.now(datetime.timezone.utc).replace(tzinfo=None)-datetime.timedelta(hours=6)
  execute('insert into Periodos (Codigo,InicioMatricula,FinMatricula,LimiteRetiro) values (?,?,?,?)','TEST-P',(now-datetime.timedelta(days=1)).isoformat(' '),(now+datetime.timedelta(days=1)).isoformat(' '),(now+datetime.timedelta(days=2)).isoformat(' '))
  pid=scalar('select Id from Periodos where Codigo=?','TEST-P')
  execute('insert into Grupos (Codigo,CursoId,DocenteId,PeriodoId,Cupo,Activo) values (?,?,?,?,?,?)','G1',courseid,tid,pid,30,1);gid=scalar('select Id from Grupos where Codigo=?','G1')
  execute('insert into Matriculas (Numero,EstudianteId,PeriodoId,CarreraHistoricaId,Fecha,Estado) values (?,?,?,?,?,?)','TEST-M',sid,pid,career1,now.isoformat(' '),'Confirmada');mid=scalar('select Id from Matriculas where Numero=?','TEST-M')
  execute('insert into DetallesMatricula (MatriculaId,GrupoId,Estado) values (?,?,?)',mid,gid,'Activo')
  r=post(admin,f'/Docentes/Estado/{tid}');check('HU14-3','Docente con grupos activos en período abierto no se desactiva','Reasígnelos' in r.text and scalar('select Activo from Docentes where Id=?',tid)==1)
  execute('update Grupos set DocenteId=(select Id from Docentes where Identificacion=?) where Id=?','DOC-DEMO',gid)
  r=post(admin,f'/Docentes/Estado/{tid}');check('HU14-BAJA','Docente reasignado puede desactivarse',r.status_code==302 and scalar('select Activo from Docentes where Id=?',tid)==0)
  edit={'Identificacion':'TEST-001','Nombre':'Prueba Estudiante editada','Correo':'prueba@campus.local','Telefono':'8777-2222','Direccion':'Alajuela','CarreraId':career2}
  r=post(admin,f'/Estudiantes/Edit/{sid}',edit);check('HU11-2A','Cambio de carrera con matrícula requiere revisión','Confirme la revisión' in r.text and scalar('select CarreraId from Estudiantes where Id=?',sid)==career1)
  r=post(admin,f'/Estudiantes/Edit/{sid}',{**edit,'RevisionCambioCarrera':'true'});check('HU11-1EDIT','Revisión permite cambio sin alterar carrera histórica',r.status_code==302 and scalar('select CarreraId from Estudiantes where Id=?',sid)==career2 and scalar('select CarreraHistoricaId from Matriculas where Id=?',mid)==career1)
  student=login('prueba@campus.local','Prueba!2026')
  r=post(admin,f'/Estudiantes/Edit/{sid}',{**edit,'Identificacion':'DEMO-001'});check('HU11-2B','Edición de estudiante rechaza identificación duplicada','identificación ya está registrada' in r.text)
  r=post(admin,f'/Estudiantes/Edit/{sid}',{**edit,'Correo':'estudiante@campus.local'});check('HU11-2C','Edición de estudiante rechaza correo duplicado','correo ya está registrado' in r.text)
  r=post(admin,'/Estudiantes/Create',{**data,'Identificacion':'TEST-ADMIN','Correo':'creado@campus.local'});check('HU11-1ALTA','Administrador crea y consulta expediente',r.status_code==302 and 'TEST-ADMIN' in get(admin,'/Estudiantes').text)
  r=post(admin,f'/Estudiantes/Estado/{sid}');check('HU11-3','Desactivar estudiante conserva expediente y matrícula',r.status_code==302 and scalar('select Activo from Estudiantes where Id=?',sid)==0 and scalar('select count(*) from Matriculas where Id=?',mid)==1)
  r=post(admin,f'/Cursos/Estado/{courseid}');check('HU13-3','Desactivar curso conserva grupos y matrículas',r.status_code==302 and scalar('select Activo from Cursos where Id=?',courseid)==0 and scalar('select count(*) from Grupos where Id=?',gid)==1 and scalar('select count(*) from DetallesMatricula where MatriculaId=?',mid)==1)
  r=post(admin,f'/Carreras/Estado/{career1}');check('HU12-3','Desactivar carrera conserva estudiantes, cursos y matrículas',r.status_code==302 and scalar('select Activa from Carreras where Id=?',career1)==0 and scalar('select count(*) from Matriculas where Id=?',mid)==1)
  check('HU04-INACTIVA','Catálogo no muestra carreras inactivas','Ingeniería en Sistemas' not in get(anon,'/Catalogo').text)
  before=scalar('select count(*) from AspNetUsers');r=post(anon,'/Account/Register',{**data,'Identificacion':'INACTIVA','Correo':'inactiva@campus.local'});check('HU01-2B','Carrera inactiva enviada manualmente no crea cuenta parcial','Seleccione una carrera activa' in r.text and scalar('select count(*) from AspNetUsers')==before)
  r=post(admin,f'/Estudiantes/Acceso/{sid}');check('HU20-1','Bloquear cuenta conserva expediente',r.status_code==302 and scalar('select Habilitado from AspNetUsers where Id=?',uid)==0 and scalar('select count(*) from Estudiantes where Id=?',sid)==1)
  r=student.get(base+'/Perfil',allow_redirects=False);check('HU20-2A','Sesión previa queda sin autorización inmediatamente',r.status_code==302 and '/Account/Login' in r.headers['Location'])
  r=post(anon,'/Account/Login',{'Correo':'prueba@campus.local','Password':'Prueba!2026'});check('HU20-2B','Cuenta bloqueada no puede iniciar sesión','No fue posible ingresar' in r.text and r.status_code==200)
  r=post(admin,f'/Estudiantes/Acceso/{sid}');student=login('prueba@campus.local','Prueba!2026');check('HU20-REACTIVAR','Habilitar acceso permite nuevo ingreso',r.status_code==302 and 'Mi portal académico' in get(student,'/').text)
  r=post(student,'/Account/Logout',tokenpath='/');r=student.get(base+'/Perfil',allow_redirects=False);check('HU02-3','Cerrar sesión impide volver a pantalla protegida',r.status_code==302 and '/Account/Login' in r.headers['Location'])
  connection=sqlite3.connect(dbfile)
  try:check('BD-01','Claves foráneas consistentes',connection.execute('PRAGMA foreign_key_check').fetchall()==[])
  finally:connection.close()
 finally:
   record();server.kill()
   server.wait(timeout=10)
   log.seek(0);Path(args.report).with_suffix('.servidor.log').write_text(log.read(),encoding='utf-8');log.close()
