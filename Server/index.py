import os
import mimetypes
from flask import Flask
from flask.globals import request
from flask.helpers import send_file
from flask.wrappers import Response

app = Flask(__name__)

@app.route('/upload', methods=["POST"])
def do_upload():

    upload     = request.files.get('image')
    name, ext = os.path.splitext(upload.filename)

    return Response(
        response = upload.read(),
        mimetype = 'image/*'
    )

app.run(host='0.0.0.0')