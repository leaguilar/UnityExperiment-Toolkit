from flask import Flask,request,jsonify
from flask_cors import CORS,cross_origin
import packageHandler
app = Flask(__name__)

CORS(app, support_credentials=True)
@app.route('/',methods=['OPTIONS', 'POST'])
@cross_origin(supports_credentials=True)
def handlePackages():
    content = request.get_json(silent=True)
    packageHandler.HanglePackage(content)
    return ""
    #response = flask.jsonify({})
    #response.headers.add('Access-Control-Allow-Origin', '*')
    #response.headers.add("Access-Control-Allow-Methods", "POST, GET, OPTIONS, PUT, DELETE")
    #response.headers.add("Access-Control-Allow-Headers", "Accept, Content-Type, Content-Length, Accept-Encoding, X-CSRF-Token, Authorization")
    #return response

@app.route('/',methods=['GET'])
def greeting():
    return "Welcome to the DeSciL experiment"
    

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=8080)
