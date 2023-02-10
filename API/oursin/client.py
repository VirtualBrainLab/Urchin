"""Client for communicating with the echo server"""
import socketio
import os

class bcolors:
    WARNING = '\033[93m'
    FAIL = '\033[91m'

sio = socketio.Client()
@sio.event
def connect():
	print("(URN) connected to server")
	ID = os.getlogin()
	change_id(ID)

@sio.event
def disconnect():
    print("(URN) disconnected from server")

log_messages = True
@sio.on('log')
def message(data):
	if log_messages:
		print('(Renderer) ' + data)

@sio.on('log-warning')
def message(data):
	if log_messages:
		print('(Renderer) ' + bcolors.WARNING + data)

@sio.on('log-error')
def message(data):
	print('(Renderer) ' + bcolors.FAIL + data)

def connected():
	return sio.connected

def close():
	"""Disconnect from the echo server
	"""
	sio.disconnect()

def change_id(newID):
	"""Change the ID used to connect to the echo server

	Parameters
	----------
	newID : string
		New ID to connect with
	"""
	sio.emit('ID',[newID,"send"])
	print("Login sent with ID: " + newID)