# -*- coding: utf-8 -*-
"""
Created on Sat Mar 16 23:59:59 2019

@author: TLSWM
"""

from flask import Flask,request,redirect,jsonify
app = Flask(__name__)
import sqlite3
from sqlite3 import Error
import smtplib, ssl
def create_connection(db_file):
    try:
        conn = sqlite3.connect(db_file)
        create_table(db_file)
    except Error as e:
        print(e)
    finally:
        conn.close()
def create_table(db_file):
    try:        
        conn = sqlite3.connect(db_file)
        cur =conn.cursor()
        try:
            cur.execute('''CREATE TABLE INQUIRIES (
    ID INTEGER PRIMARY KEY,
    NAME TEXT,
    AGE INTEGER, PHONE TEXT, EMAIL TEXT );''')
        except Error as e:
            print(e)
        try:
            cur.execute('''CREATE TABLE Information (
    ID TEXT PRIMARY KEY,
    NAME TEXT,
    SIZE INTEGER, PRICE TEXT);''')
        except Error as e:
            print(e)
    except Error as e:
        print(e)
    finally:
        conn.close()
@app.route('/upload-inquiry/',methods = ['POST', 'GET'])  
def getentiredata():
    if request.method == 'POST':
        ID = request.form['ID']
        NAME = request.form['NAME']
        AGE = request.form['AGE']
        PHONE = request.form['PHONE']
        EMAIL = request.form['EMAIL']
    else:
        ID = request.args.get('ID')
        NAME = request.args.get('NAME')
        AGE = request.args.get('AGE')
        PHONE = request.args.get('PHONE')
        EMAIL = request.args.get('EMAIL')
    if recordsentry(ID,NAME,AGE,PHONE,EMAIL):
        return "Data entry done"
    else:
        return "Data entry Not done"
def recordsentry(ID,NAME,AGE,PHONE,EMAIL):
    conn = sqlite3.connect("database.db")
    qry="insert into INQUIRIES values(?,?,?,?,?);"
    try:
        cur=conn.cursor()
        cur.execute(qry,(ID,NAME,AGE,PHONE,EMAIL))
        conn.commit()
        print ("one record added successfully")
        conn.close()
        return True
    except:
        print("error in operation")
        conn.rollback()
        conn.close()
        return False
@app.route('/getinfo/<string:ID>/')
def dbinfo(ID):
    db=sqlite3.connect('database.db')
    sql="SELECT * from Information where ID ='"+ID+"' ;"
    cur=db.cursor()
    cur.execute(sql)
    records=[]
    while True:
        record=cur.fetchone()
        if record==None:
            break
        records=record
    db.close()
    return "\n".join(["The "+str(x)+ " is "+str(y)+"." for (x,y) in zip(["ID","NAME","SIZE","PRICE"],records)])
@app.route('/buy/<string:det>/<string:ID>/')
def mailsend(det,ID): 
    port = 587  # For starttls
    smtp_server = "smtp.gmail.com"
    sender_email = "tlonesamurai@gmail.com"
    receiver_email = "infantsamchris@gmail.com"
    password = "livhacksample"
    message = "\n"+det + inforet(ID)
    context = ssl.create_default_context()
    with smtplib.SMTP(smtp_server, port) as server:
        server.ehlo()  # Can be omitted
        server.starttls(context=context)
        server.ehlo()  # Can be omitted
        server.login(sender_email, password)
        server.sendmail(sender_email, receiver_email, message)
    return "Done"
def inforet(ID):
    db=sqlite3.connect('database.db')
    sql="SELECT * from INQUIRIES where ID ="+ID+" ;"
    cur=db.cursor()
    cur.execute(sql)
    records=[]
    while True:
        record=cur.fetchone()
        if record==None:
            break
        records=record
    db.close()
    return "\n".join(["The "+str(x)+ " is "+str(y)+"." for (x,y) in zip(["ID","NAME","AGE","PHONE","EMAIL"],records)])

@app.route('/')
def hello():
    return "Hello World!"
if __name__ == '__main__':
    create_connection("database.db")
    app.run(debug=True, host='192.168.43.164', port="5000")
