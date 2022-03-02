const express = require("express");
const socket = require("socket.io");
// const cors = require('cors');

// App setup
const port = process.env.PORT || 5000
const app = express();
const server = app.listen(port, function () {
  console.log(`Listening on port ${port}`);
});

app.use(function(req, res, next) {
    res.header("Access-Control-Allow-Origin", '*');
    res.header("Access-Control-Allow-Credentials", true);
    res.header('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE,OPTIONS');
    res.header("Access-Control-Allow-Headers", 'Origin,X-Requested-With,Content-Type,Accept,content-type,application/json');
    next();
});
// Static files
app.use(express.static("public"));
// app.use(cors());

// Socket setup
const io = socket(server);

idDict = {}; // keeps track of all sockets with the same ID
clientIDdict = {}; // keeps track of the ID of each socket

io.on("connection", function (socket) {
  console.log("Client connected with ID: " + socket.id);

  socket.on('disconnect', () => {
  	console.log('Client disconnected with ID: ' + socket.id);

  	if (idDict[clientIDdict[socket.id]]) {
		idDict[clientIDdict[socket.id]].splice(idDict[clientIDdict[socket.id]].indexOf(socket.id),1);
		clientIDdict[socket.id] = undefined;
  	}
  })

  socket.on('ID', function(newClientID) {
  	console.log('Client ' + socket.id + ' requested to update ID to ' + newClientID);
  	// Check if we have an old clientID that needs to be removed
  	if (clientIDdict[socket.id]) {
  		oldClientID = clientIDdict[socket.id]

  		idDict[oldClientID].splice(idDict[oldClientID].indexOf(socket.id),1);
  	}

  	if (idDict[newClientID] == undefined) {
  		// save the new entry into a new list
  		idDict[newClientID] = [socket.id];
  	}
  	else {
  		// save the new entry
  		idDict[newClientID].push(socket.id);
  	}
  	// update the client ID locally
	clientIDdict[socket.id] = newClientID;
  	console.log('User updated their ID to: ' + clientIDdict[socket.id]);
  	console.log('All connected clients with ID: ' + idDict[clientIDdict[socket.id]]);
  });

  socket.on('SetVolumeColors', function(data) {
  	emitToAll(socket.id, 'SetVolumeColors', data);
  });
  socket.on('SetVolumeVisibility', function(data) {
  	emitToAll(socket.id, 'SetVolumeVisibility', data);
  });
  socket.on('SetVolumeIntensity', function(data) {
    emitToAll(socket.id, 'SetVolumeIntensity', data);
  });
  socket.on('SetVolumeColormap', function(data) {
    emitToAll(socket.id, 'SetVolumeColormap', data);
  });
  socket.on('SetVolumeStyle', function(data) {
    emitToAll(socket.id, 'SetVolumeStyle', data);
  });
  socket.on('SetVolumeShader', function(data) {
    emitToAll(socket.id, 'SetVolumeShader', data);
  });
  socket.on('SetVolumeAlpha', function(data) {
    emitToAll(socket.id, 'SetVolumeAlpha', data);
  });
  socket.on('CreateNeurons', function(data) {
    emitToAll(socket.id, 'CreateNeurons', data);
  });
  socket.on('SetNeuronPos', function(data) {
    emitToAll(socket.id, 'SetNeuronPos', data);
  });
  socket.on('SetNeuronSize', function(data) {
    emitToAll(socket.id, 'SetNeuronSize', data);
  });
  socket.on('SetNeuronShape', function(data) {
    emitToAll(socket.id, 'SetNeuronShape', data);
  });
  socket.on('SetNeuronColor', function(data) {
    emitToAll(socket.id, 'SetNeuronColor', data);
  });
  socket.on('SliceVolume', function(data) {
    emitToAll(socket.id, 'SliceVolume', data);
  });
  socket.on('SetSliceColor', function(data) {
    emitToAll(socket.id, 'SetSliceColor', data);
  });
  socket.on('CreateProbes', function(data) {
    emitToAll(socket.id, 'CreateProbes', data);
  });
  socket.on('SetProbeColors', function(data) {
    emitToAll(socket.id, 'SetProbeColors', data);
  });
  socket.on('SetProbePos', function(data) {
    emitToAll(socket.id, 'SetProbePos', data);
  });
  socket.on('SetProbeAngles', function(data) {
    emitToAll(socket.id, 'SetProbeAngles', data);
  });
  socket.on('SetProbeStyle', function(data) {
    emitToAll(socket.id, 'SetProbeStyle', data);
  });
  socket.on('SetProbeSize', function(data) {
    emitToAll(socket.id, 'SetProbeSize', data);
  });
  socket.on('SetProbeSize', function(data) {
    emitToAll(socket.id, 'SetProbeSize', data);
  });
  socket.on('ClearAll', function(data) {
    emitToAll(socket.id, 'ClearAll', data);
  });
  socket.on('LoadDefaultAreas', function(data) {
    emitToAll(socket.id, 'LoadDefaultAreas', data);
  });
});

function emitToAll(id, event, data) {
  	console.log('User sent data: ' + data + ' emitting to all clients with ID: ' + clientIDdict[id]);
  	for (var socketID of idDict[clientIDdict[id]]) {
  		console.log('Emitting to: ' + socketID);
  		io.to(socketID).emit(event,data);
  	}
}