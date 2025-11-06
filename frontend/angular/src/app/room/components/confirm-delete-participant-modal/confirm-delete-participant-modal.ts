import { Component, input, output } from '@angular/core';

import { CommonModalTemplate } from '../../../shared/components/modal/common-modal-template/common-modal-template';
import {
  ButtonText,
  ModalSubtitle,
  ModalTitle,
  PictureName,
} from '../../../app.enum';
import { RemoveParticipantModalInputs } from '../../../app.models';

@Component({
  selector: 'app-confirm-delete-participant-modal',
  imports: [CommonModalTemplate],
  templateUrl: './confirm-delete-participant-modal.html',
  styleUrl: './confirm-delete-participant-modal.scss',
})
export class ConfirmDeleteParticipantModal {
  readonly id = input.required<RemoveParticipantModalInputs>();
  readonly name = input.required<RemoveParticipantModalInputs>();

  public readonly pictureName = PictureName.Attention;
  public readonly title = ModalTitle.RemoveParticipant;
  public readonly subtitle = ModalSubtitle.RemoveParticipant;
  public readonly buttonText = ButtonText.Remove;
  public readonly cancelButtonText = ButtonText.Cancel;

  readonly closeModal = output<void>();
  readonly buttonAction = output<void>();

  public onActionButtonClick(): void {
    console.log(this.id());
    this.buttonAction.emit();
  }

  public onCloseModal(): void {
    this.closeModal.emit();
  }
}
