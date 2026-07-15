from django import forms

from .models import Enrollment


class EnrollmentForm(forms.ModelForm):
    class Meta:
        model = Enrollment
        fields = ["session", "name", "email", "phone", "notes"]

    def __init__(self, *args, offering=None, **kwargs):
        super().__init__(*args, **kwargs)
        if offering is not None:
            self.fields["session"].queryset = offering.sessions.all()
        self.fields["session"].required = False
