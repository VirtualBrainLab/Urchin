const express = require("express");
const socket = require("socket.io");
const cors = require("cors");

// App setup
const port = process.env.PORT || 5000
const app = express();

app.use(cors());

const server = app.listen(port, function () {
  console.log(`Listening on port ${port}`);
});

// Socket setup
const io = require("socket.io")(server, {
  cors:
  {
    "origin": "http://data.virtualbrainlab.org",
    "methods": "GET,HEAD,PUT,PATCH,POST,DELETE",
    "preflightContinue": false,
    "optionsSuccessStatus": 204
  } 
});

ID2Socket = {}; // keeps track of all sockets with the same ID
Socket2ID = {}; // keeps track of the ID of each socket
Socket2Type = {};

io.on("connection", function (socket) {
  console.log("Client connected with ID: " + socket.id);

  socket.on('disconnect', () => {
  	console.log('Client disconnected with ID: ' + socket.id);

  	if (ID2Socket[Socket2ID[socket.id]]) {
      ID2Socket[Socket2ID[socket.id]].splice(ID2Socket[Socket2ID[socket.id]].indexOf(socket.id),1);
      Socket2ID[socket.id] = undefined;
  	}
  })

  socket.on('ID', function(clientData) {
    // ID is just a unique identifier, it can be any string
    newClientID = clientData[0]
    // Type can be "send" or "receive"
    newClientType = clientData[1]


  	console.log('Client ' + socket.id + ' requested to update ID to ' + newClientID);
  	// Check if we have an old clientID that needs to be removed
  	if (Socket2ID[socket.id]) {
  		oldClientID = Socket2ID[socket.id]

  		ID2Socket[oldClientID].splice(ID2Socket[oldClientID].indexOf(socket.id),1);
  	}

  	if (ID2Socket[newClientID] == undefined) {
  		// save the new entry into a new list
  		ID2Socket[newClientID] = [socket.id];
  	}
  	else {
  		// save the new entry
  		ID2Socket[newClientID].push(socket.id);
  	}
  	// update the client ID locally
	  Socket2ID[socket.id] = newClientID;
    Socket2Type[socket.id] = newClientType
  	console.log('User updated their ID to: ' + Socket2ID[socket.id] + " type " + newClientType );
  	console.log('All connected clients with ID: ' + ID2Socket[Socket2ID[socket.id]]);
  });

  socket.on('SetAreaColors', function(data) {
  	emitToAll(socket.id, 'SetAreaColors', data);
  });
  socket.on('SetAreaVisibility', function(data) {
  	emitToAll(socket.id, 'SetAreaVisibility', data);
  });
  socket.on('SetAreaIntensity', function(data) {
    emitToAll(socket.id, 'SetAreaIntensity', data);
  });
  socket.on('SetAreaColormap', function(data) {
    emitToAll(socket.id, 'SetAreaColormap', data);
  });
  socket.on('SetAreaShader', function(data) {
    emitToAll(socket.id, 'SetAreaShader', data);
  });
  socket.on('SetAreaAlpha', function(data) {
    emitToAll(socket.id, 'SetAreaAlpha', data);
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
  socket.on('SetCameraTarget', function(data) {
    emitToAll(socket.id, 'SetCameraTarget', data);
  });
  socket.on('SetCameraTargetArea', function(data) {
    emitToAll(socket.id, 'SetCameraTargetArea', data);
  });
  socket.on('SetCameraYAngle', function(data) {
    emitToAll(socket.id, 'SetCameraYAngle', data);
  });
  socket.on('LoadDefaultAreas', function(data) {
    emitToAll(socket.id, 'LoadDefaultAreas', data);
  });
  socket.on('SetVolumeVisibility', function(data) {
    emitToAll(socket.id, 'SetVolumeVisibility', data);
  });
  socket.on('ClearAll', function(data) {
    emitToAll(socket.id, 'ClearAll', data);
  });
});

function emitToAll(id, event, data) {
  	console.log('User sent event: ' + event + ' emitting to all clients with ID: ' + Socket2ID[id] + " and type receive");
  	for (var socketID of ID2Socket[Socket2ID[id]]) {
      if (Socket2Type[socketID]=="receive") {
        console.log('Emitting to: ' + socketID);
        io.to(socketID).emit(event,data);
      }
  	}
}